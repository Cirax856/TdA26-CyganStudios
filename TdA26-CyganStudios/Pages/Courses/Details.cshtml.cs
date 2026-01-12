using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Courses;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _appDb;

    public DetailsModel(AppDbContext appDb)
    {
        _appDb = appDb;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public DbCourse Course { get; set; } = null!;

    public async Task<IActionResult> OnGet()
    {
        var course = await _appDb.Courses
            .Include(course => course.Lecturer)
            .Include(course => course.Materials)
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid);

        if (course is null)
        {
            return NotFound();
        }

        Course = course;

        return Page();
    }
}
