using DotNetFlix.Identity.Data;
using DotNetFlix.Identity.Helpers;
using DotNetFlix.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetFlix.Identity.Services;

public class FidoCredentialStore : IFidoCredentialStore
{
    private readonly ApplicationDbContext _dbContext;

    public FidoCredentialStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCredentialToUserAsync(FidoUser user, FidoStoredCredential credential, CancellationToken cancellationToken = default)
    {
        user.StoredCredentials.Add(credential);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<FidoStoredCredential> GetCredentialByIdAsync(byte[] id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FidoStoredCredentials.FirstOrDefaultAsync(c => c.Id.SequenceEqual(id), cancellationToken);
    }

    public async Task<List<FidoStoredCredential>> GetCredentialsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.FidoUsers
            // .AsNoTracking?? Checck whether the result is updated and/or stored
            .FirstOrDefaultAsync(u => u.ApplicationUserId == userId, cancellationToken);

        return user.StoredCredentials.ToList();
    }

    public async Task<List<FidoPublicKeyDescriptor>> GetKeyDescriptorsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FidoUsers.Where(fu => fu.ApplicationUserId == userId)
            .SelectMany(fu => fu.StoredCredentials)
            .Select(sc => sc.Descriptor)
            .ToListAsync(cancellationToken);
    }

    public async Task<FidoUser> GetOrAddUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var usr = await _dbContext.Users
            .Include(u => u.FidoUser)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (usr is null)
            throw new Exception($"User {userId} not found");

        if (usr.FidoUser is null)
        {
            var user = new FidoUser
            {
                DisplayName = usr.UserName,
                Name = usr.UserName,
                Id = usr.Id.ToByteArray()
            };
            usr.FidoUser = user;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }
        else
        {
            return usr.FidoUser;
        }
    }

    public async Task<FidoUser> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FidoUsers.FirstOrDefaultAsync(fu => fu.ApplicationUserId == userId);
        // AsNoTracking? Check whether the result is updated.
    }

    public async Task<List<FidoUser>> GetUsersByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FidoStoredCredentials.Where(c => c.Descriptor.Id == credentialId)
            .Select(c => c.FidoUser)
            .ToListAsync(cancellationToken);

        // AsNoTracking? Check whether result is updated.
    }

    public async Task UpdateCounterAsync(byte[] credentialId, uint counter, CancellationToken cancellationToken = default)
    {
        var cred = await _dbContext.FidoStoredCredentials.FindAsync(credentialId, cancellationToken);

        cred.SignCount = counter;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
