using Fido2NetLib.Objects;

namespace DotNetFlix.Identity.Models;

public class FidoPublicKeyDescriptor : PublicKeyCredentialDescriptor
{
    public int DescriptorId { get; set; }

    public int CredentialId { get; set; }
    public FidoStoredCredential Credential { get; set; }

    public FidoPublicKeyDescriptor(byte[] CredentialId)
    {
        Id = CredentialId;
    }

    public FidoPublicKeyDescriptor()
    {
        
    }
}
