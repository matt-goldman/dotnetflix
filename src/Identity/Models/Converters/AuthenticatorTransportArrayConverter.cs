using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetFlix.Identity.Models.Converters;

public class AuthenticatorTransportArrayConverter : ValueConverter<AuthenticatorTransport[], string>
{
    public AuthenticatorTransportArrayConverter() : base(
        v => string.Join(",", v),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
               .Select(s => Enum.Parse<AuthenticatorTransport>(s))
               .ToArray())
    {
    }
}

