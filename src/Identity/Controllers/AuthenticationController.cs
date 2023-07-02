using DotNetFlix.Identity.Helpers;
using DotNetFlix.Identity.Models;
using DotNetFlix.Identity.Services;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DotNetFlix.Identity.Controllers;

public class AuthenticationController : ControllerBase
{
    private readonly IFidoCredentialStore _credentialStore;
    private readonly IFido2 _fido2;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthenticationController(
        IFidoCredentialStore credentialStore,
        IFido2 fido2,
        SignInManager<ApplicationUser> signInManager)
    {
        _credentialStore = credentialStore;
        _fido2 = fido2;
        _signInManager = signInManager;
    }
    
    [HttpPost]
    [Route("/assertionOptions")] // This route returns options to the browser that are passed to the navigator.credentials.get() method.
    public async Task<AssertionOptions> AssertionOptionsPost([FromForm] string username, [FromForm] string userVerification)
    {
        try
        {
            var existingCredentials = new List<FidoPublicKeyDescriptor>();
            
            if (username is not null)
            {
                existingCredentials = await _credentialStore.GetKeyDescriptorsByUserIdAsync(username);
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
            return new AssertionOptions { Status = "error", ErrorMessage = e.StringFormat() };
        }
    }

    [HttpPost]
    [Route("/makeAssertion")] // This route handles the response from the browser after the user has completed the navigator.credentials.get() method.
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
            // NOTE: This method will throw an exception if the WebAuthN authentication fails.
            var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, creds.DevicePublicKeys, storedCounter, callback, cancellationToken: cancellationToken);

            // 6. Store the updated counter
            await _credentialStore.UpdateCounterAsync(res.CredentialId, res.Counter);

            if (res.DevicePublicKey is not null)
                creds.DevicePublicKeys.Add(res.DevicePublicKey);

            // 7. If we've got this far without an exception, the assertion is valid
            // Sign the user in. This gives them a cookie, and means that now they
            // can obtain a token via standard OIDC processes.
            await _signInManager.SignInAsync(creds.FidoUser.ApplicationUser, true);

            // 8. return OK to client
            return res;
        }
        catch (Exception e)
        {
            return new AssertionVerificationResult { Status = "error", ErrorMessage = e.StringFormat() };
        }
    }

    public IsUserHandleOwnerOfCredentialIdAsync CreateCredentialCallback(string userId)
    {
        return async (args, cancellationToken) =>
        {
            var storedCreds = await _credentialStore.GetCredentialsByUserIdAsync(userId, cancellationToken);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        };
    }
}
