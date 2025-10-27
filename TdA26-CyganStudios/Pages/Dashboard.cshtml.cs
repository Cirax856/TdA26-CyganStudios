using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TdA26_CyganStudios.Pages;

[Authorize(Roles = "lecturer")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}
