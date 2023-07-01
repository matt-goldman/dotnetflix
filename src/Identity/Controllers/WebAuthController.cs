using DotNetFlix.Identity.Models;
using DotNetFlix.Identity.Services;
using Duende.IdentityServer.Extensions;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static Fido2NetLib.Fido2;

namespace DotNetFlix.Identity.Controllers;

public class WebAuthController : ControllerBase
{
    private readonly IFidoCredentialStore _credentialStore;
    private readonly IFido2 _fido2;

    public WebAuthController(IFidoCredentialStore credentialStore, IFido2 fido2)
    {
        _credentialStore = credentialStore;
        _fido2 = fido2;
    }

    [HttpPost]
    [Authorize]
    [Route("/makeCredentialOptions")]
    public async Task<CredentialCreateOptions> MakeCredentialOptions(
                                            [FromForm] string attType,
                                            [FromForm] string authType,
                                            [FromForm] string residentKey,
                                            [FromForm] string userVerification)
    {
        try
        {
            var userId = User?.Identity.GetSubjectId();

            // 1. Get user from DB by username (in our example, auto create missing users)
            var user = await _credentialStore.GetOrAddUserAsync(userId);

            // 2. Get user existing keys by username
            var existingKeys = await _credentialStore.GetKeyDescriptorsByUserIdAsync(userId);

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = residentKey.ToEnum<ResidentKeyRequirement>(),
                UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
            };

            if (!string.IsNullOrEmpty(authType))
                authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

            var exts = new AuthenticationExtensionsClientInputs()
            {
                Extensions = true,
                UserVerificationMethod = true,
                DevicePubKey = new AuthenticationExtensionsDevicePublicKeyInputs() { Attestation = attType },
                CredProps = true
            };

            var options = _fido2.RequestNewCredential(user, existingKeys.Cast<PublicKeyCredentialDescriptor>().ToList(), authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);

            // 4. Temporarily store options, session/in-memory cache/redis/db
            HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());
            var jsonUser = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString("fido2.attestationUser", jsonUser);

            // 5. return options to client
            return options;
        }
        catch (Exception e)
        {
            return new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) };
        }
    }

    [HttpPost]
    [Route("/makeCredential")]
    public async Task<CredentialMakeResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse, CancellationToken cancellationToken)
    {
        try
        {
            // 1. get the options we sent the client
            var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            // 2. Get the user we created
            var jsonUser = HttpContext.Session.GetString("fido2.attestationUser");
            var user = JsonSerializer.Deserialize<FidoUser>(jsonUser);
            
            // 2. Create callback so that lib can verify credential id is unique to this user
            var callback = CreateCallback();

            // 2. Verify and make the credentials
            var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options, callback, cancellationToken: cancellationToken);

            // 3. Store the credentials in db
            await _credentialStore.AddCredentialToUserAsync(user, new FidoStoredCredential
            {
                Type = success.Result.Type,
                Id = success.Result.Id,
                Descriptor = new FidoPublicKeyDescriptor(success.Result.CredentialId),
                PublicKey = success.Result.PublicKey,
                UserHandle = success.Result.User.Id,
                SignCount = success.Result.Counter,
                CredType = success.Result.CredType,
                RegDate = DateTime.Now,
                AaGuid = success.Result.AaGuid,
                Transports = success.Result.Transports,
                BE = success.Result.BE,
                BS = success.Result.BS,
                AttestationObject = success.Result.AttestationObject,
                AttestationClientDataJSON = success.Result.AttestationClientDataJSON,
                DevicePublicKeys = new List<byte[]>() { success.Result.DevicePublicKey }
            });

            // 4. return "ok" to the client
            return success;
        }
        catch (Exception e)
        {
            return new CredentialMakeResult(status: "error", errorMessage: FormatException(e), result: null);
        }
    }

    [HttpPost]
    [Route("/assertionOptions")]
    public async Task<AssertionOptions> AssertionOptionsPost([FromForm] string userVerification)
    {
        try
        {
            var existingCredentials = new List<FidoPublicKeyDescriptor>();

            var userId = User?.Identity.GetSubjectId();

            if (userId is not null)
            {
                existingCredentials = await _credentialStore.GetKeyDescriptorsByUserIdAsync(userId);
            }

            var exts = new AuthenticationExtensionsClientInputs()
            {
                Extensions = true,
                UserVerificationMethod = true,
                DevicePubKey = new AuthenticationExtensionsDevicePublicKeyInputs()
            };

            // 3. Create options
            var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
            var options = _fido2.GetAssertionOptions(
                existingCredentials,
                uv,
                exts
            );

            // 4. Temporarily store options, session/in-memory cache/redis/db
            HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());

            // 5. Return options to client
            return options;
        }

        catch (Exception e)
        {
            return new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) };
        }
    }

    [HttpPost]
    [Route("/makeAssertion")]
    public async Task<AssertionVerificationResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the assertion options we sent the client
            var jsonOptions = HttpContext.Session.GetString("fido2.assertionOptions");
            var options = AssertionOptions.FromJson(jsonOptions);

            var jsonUser = HttpContext.Session.GetString("fido2.attestationUser");
            var user = JsonSerializer.Deserialize<FidoUser>(jsonUser);

            // 2. Get registered credential from database
            var creds = await _credentialStore.GetCredentialByIdAsync(clientResponse.Id) ?? throw new Exception("Unknown credentials");

            // 3. Get credential counter from database
            var storedCounter = creds.SignCount;

            // 4. Create callback to check if userhandle owns the credentialId
            var callback = CreateCredentialCallback(user.ApplicationUserId);

            // 5. Make the assertion
            var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, creds.DevicePublicKeys, storedCounter, callback, cancellationToken: cancellationToken);

            // 6. Store the updated counter
            await _credentialStore.UpdateCounterAsync(res.CredentialId, res.Counter);

            if (res.DevicePublicKey is not null)
                creds.DevicePublicKeys.Add(res.DevicePublicKey);

            // 7. return OK to client
            return res;
        }
        catch (Exception e)
        {
            return new AssertionVerificationResult { Status = "error", ErrorMessage = FormatException(e) };
        }
    }

    private string FormatException(Exception e)
    {
        return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
    }

    public IsCredentialIdUniqueToUserAsyncDelegate CreateCallback()
    {
        return async (args, cancellationToken) =>
        {
            var users = await _credentialStore.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
            return users.Count == 0;
        };
    }

    public IsUserHandleOwnerOfCredentialIdAsync CreateCredentialCallback(string userId)
    {
        return async (args, cancellationToken) =>
        {
            var storedCreds = await _credentialStore.GetCredentialsByUserHandleAsync(userId, cancellationToken);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        };
    }
}
