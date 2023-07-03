using Fido2NetLib;

namespace DotNetFlix.Identity.Models;

public class FidoUser : Fido2User
{
    public int UserId { get; set; }

    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    public ICollection<FidoStoredCredential> StoredCredentials { get; set; } = new List<FidoStoredCredential>();
}
