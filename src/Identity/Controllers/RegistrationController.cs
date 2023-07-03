using DotNetFlix.Identity.Helpers;
using DotNetFlix.Identity.Models;
using DotNetFlix.Identity.Services;
using Duende.IdentityServer.Extensions;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using static Fido2NetLib.Fido2;

namespace DotNetFlix.Identity.Controllers;

[ApiController]
[Authorize]
public class RegistrationController : ControllerBase
{
    private readonly IFidoCredentialStore _credentialStore;
    private readonly IFido2 _fido2;

    public RegistrationController(IFidoCredentialStore credentialStore, IFido2 fido2)
    {
        _credentialStore = credentialStore;
        _fido2 = fido2;
    }
    
    [HttpPost]
    [Route("/makeCredentialOptions")] // This route returns options to the browser that are passed to the navigator.credentials.create() method.
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
            
            HttpContext.Session.SetString("fido2.attestationUser", userId);

            // 5. return options to client
            return options;
        }
        catch (Exception e)
        {
            return new CredentialCreateOptions { Status = "error", ErrorMessage = e.StringFormat() };
        }
    }

    [HttpPost]
    [Route("/makeCredential")] // This route handles the response from the browser after the user has completed the navigator.credentials.create() method.
    public async Task<ActionResult<CredentialMakeResult>> MakeCredential([FromBody] AttestationResponse attestationResponse, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. get the options we sent the client
            var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            // 2. Get the user we created
            var userId = HttpContext.Session.GetString("fido2.attestationUser");
            var user = await _credentialStore.GetUserAsync(userId);

            // 2. Create callback so that lib can verify credential id is unique to this user
            var callback = CreateCallback();

            var fidoResponse = new AuthenticatorAttestationRawResponse
            {
                Id = attestationResponse.Id,
                RawId = attestationResponse.RawId,
                Type = attestationResponse.Type,
                Response = new AuthenticatorAttestationRawResponse.ResponseData
                {
                    AttestationObject = attestationResponse.Response.AttestationObject,
                    ClientDataJson = attestationResponse.Response.ClientDataJson
                },
                Extensions = attestationResponse.Extensions
            };

            // 2. Verify and make the credentials
            var success = await _fido2.MakeNewCredentialAsync(fidoResponse, options, callback, cancellationToken: cancellationToken);

            // 3. Store the credentials in db
            await _credentialStore.AddCredentialToUserAsync(user, new FidoStoredCredential
            {
                Type = success.Result.Type,
                Id = success.Result.Id,
                Descriptor = new FidoPublicKeyDescriptor(success.Result.Id),
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
            return new CredentialMakeResult(status: "error", errorMessage: e.StringFormat(), result: null);
        }
    }

    private IsCredentialIdUniqueToUserAsyncDelegate CreateCallback()
    {
        return async (args, cancellationToken) =>
        {
            var users = await _credentialStore.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
            return users.Count == 0;
        };
    }
}

// This is required for the FIDO2 library to be able to deserialize the response from the browser.
// The CredProps property is a bool, but the navigator.credentials.create() method returns an object.
public class AuthenticationExtensionOutputs : AuthenticationExtensionsClientOutputs
{
    [JsonPropertyName("credProps")]
    public new CredProps CredProps { get; set; }

    
}

public class CredProps
{
    [JsonPropertyName("rk")]
    public bool Rk { get; set; }
}

public class AttestationResponse
{
    [JsonConverter(typeof(Base64UrlConverter))]
    [JsonPropertyName("id")]
    public byte[] Id { get; set; }

    [JsonConverter(typeof(Base64UrlConverter))]
    [JsonPropertyName("rawId")]
    public byte[] RawId { get; set; }

    [JsonPropertyName("type")]
    public PublicKeyCredentialType Type { get; set; } = PublicKeyCredentialType.PublicKey;

    [JsonPropertyName("response")]
    public ResponseData Response { get; set; }

    [JsonPropertyName("extensions")]
    public AuthenticationExtensionOutputs Extensions { get; set; }

    public sealed class ResponseData
    {
        [JsonConverter(typeof(Base64UrlConverter))]
        [JsonPropertyName("attestationObject")]
        public byte[] AttestationObject { get; set; }

        [JsonConverter(typeof(Base64UrlConverter))]
        [JsonPropertyName("clientDataJSON")]
        public byte[] ClientDataJson { get; set; }
    }
}
