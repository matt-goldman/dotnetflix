using Fido2NetLib.Development;
using Fido2NetLib.Objects;

namespace DotNetFlix.Identity.Models;

public class FidoStoredCredential : StoredCredential
{
    public int Id { get; set; }
    
    public FidoUser User { get; set; }
}
