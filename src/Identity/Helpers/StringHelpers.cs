using System.Text;

namespace DotNetFlix.Identity.Helpers;

public static class StringHelpers
{
    public static byte[] ToByteArray(this string inputString)
    {
        return Encoding.UTF8.GetBytes(inputString);
    }
}
