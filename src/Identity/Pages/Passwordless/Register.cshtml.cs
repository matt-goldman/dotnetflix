using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotNetFlix.Identity.Pages.Passwordless;

[Authorize]
public class RegisterModel : PageModel
{
    public void OnGet()
    {
    }
}
