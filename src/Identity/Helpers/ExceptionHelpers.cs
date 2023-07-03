namespace DotNetFlix.Identity.Helpers;

public static class ExceptionHelpers
{
    public static string StringFormat(this Exception e)
    {
        return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
    }
}
