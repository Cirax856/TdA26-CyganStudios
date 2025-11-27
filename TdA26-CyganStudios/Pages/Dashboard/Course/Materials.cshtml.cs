using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]
public class MaterialsModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<CourseNewModel> _logger;

    public MaterialsModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<CourseNewModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public DbCourse Course { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var course = await _appDb.Courses
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