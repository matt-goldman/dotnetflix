using Fido2NetLib.Development;

namespace DotNetFlix.Identity.Models;

public class FidoStoredCredential : StoredCredential
{
    public int CredentialId { get; set; }
    
    public int FidoUserId { get; set; }
    public FidoUser FidoUser { get; set; }

    public new FidoPublicKeyDescriptor Descriptor { get; set; }
}
