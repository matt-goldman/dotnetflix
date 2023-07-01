using Fido2NetLib;

namespace DotNetFlix.Identity.Models;

public class FidoUser : Fido2User
{
    public new int Id { get; set; }

    public string ApplicationUserId { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<FidoStoredCredential> StoredCredentials { get; set; }
}
