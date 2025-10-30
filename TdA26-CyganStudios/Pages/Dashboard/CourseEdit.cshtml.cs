using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TdA26_CyganStudios.Pages.Dashboard;

public class CourseEditModel : PageModel
{
    [BindProperty(Name = "uuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public void OnGet()
    {
        // abc
    }
}
