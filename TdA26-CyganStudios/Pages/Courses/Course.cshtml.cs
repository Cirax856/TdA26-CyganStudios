using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TdA26_CyganStudios.Pages.Courses;

public class CourseModel : PageModel
{
    [BindProperty(Name = "uuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public void OnGet()
    {
    }
}
