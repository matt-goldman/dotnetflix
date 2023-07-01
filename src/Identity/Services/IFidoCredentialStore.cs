using DotNetFlix.Identity.Models;
using Fido2NetLib.Objects;

namespace DotNetFlix.Identity.Services;

public interface IFidoCredentialStore
{
    Task<FidoUser> GetOrAddUserAsync(string userId, CancellationToken cancellationToken = default);
    
    Task<FidoUser?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
        
    Task<FidoStoredCredential?> GetCredentialByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<FidoStoredCredential>> GetCredentialsByUserHandleAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<PublicKeyCredentialDescriptor>> GetKeyDescriptorsByUserHandleAsync(string userId, CancellationToken cancellationToken = default);

    Task UpdateCounterAsync(int credentialId, uint counter, CancellationToken cancellationToken = default);
    
    Task AddCredentialToUserAsync(FidoUser user, FidoStoredCredential credential, CancellationToken cancellationToken = default);

    Task<List<FidoUser>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
}
