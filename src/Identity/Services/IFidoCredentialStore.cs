using DotNetFlix.Identity.Models;

namespace DotNetFlix.Identity.Services;

public interface IFidoCredentialStore
{
    Task<FidoUser> GetOrAddUserAsync(string userId, CancellationToken cancellationToken = default);
    
    Task<FidoUser?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
        
    Task<FidoStoredCredential?> GetCredentialByIdAsync(byte[] id, CancellationToken cancellationToken = default);

    Task<List<FidoStoredCredential>> GetCredentialsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<FidoPublicKeyDescriptor>> GetKeyDescriptorsByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task UpdateCounterAsync(byte[] credentialId, uint counter, CancellationToken cancellationToken = default);
    
    Task AddCredentialToUserAsync(FidoUser user, FidoStoredCredential credential, CancellationToken cancellationToken = default);

    Task<List<FidoUser>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
}
