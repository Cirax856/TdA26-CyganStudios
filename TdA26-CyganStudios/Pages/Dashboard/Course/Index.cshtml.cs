using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<CourseNewModel> _logger;

    public IndexModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<CourseNewModel> logger)
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
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid);

        if (course is null)
        {
            return NotFound();
        }

        if (course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        Course = course;

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid materialUuid)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Challenge();
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid);

        if (course is null)
        {
            return NotFound();
        }

        if (course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        var material = course.Materials.FirstOrDefault(material => material.Uuid == materialUuid);

        if (material is null)
        {
            return NotFound();
        }

        try
        {
            course.Materials.Remove(material);
            await _appDb.SaveChangesAsync();

            _logger.LogInformation("Material deleted.");

            TempData["SuccessMessage"] = $"Material '{material.Name}' deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting material {MaterialUuid} for user {UserId}.", materialUuid, currentUser.Id);
            TempData["ErrorMessage"] = "An error occurred while deleting the material.";
        }

        return RedirectToPage(new { courseUuid = CourseUuid });
    }
}
