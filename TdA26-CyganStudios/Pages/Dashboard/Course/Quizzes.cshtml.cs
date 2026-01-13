using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]
public class QuizzesModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<QuizzesModel> _logger;

    public QuizzesModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<QuizzesModel> logger)
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
            .Include(course => course.Quizzes)
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

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemUuid, string type)
    {
        if (type is not "quiz")
        {
            return BadRequest();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return Challenge();
        }

        var course = await _appDb.Courses
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid);

        if (course is null)
        {
            return NotFound();
        }

        if (course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        var quiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(material => material.CourseId == CourseUuid && material.Uuid == itemUuid);

        if (quiz is null)
        {
            return NotFound();
        }

        try
        {
            course.Quizzes.Remove(quiz);
            await _appDb.SaveChangesAsync();

            _logger.LogInformation("Quiz deleted.");

            TempData["SuccessMessage"] = $"Quiz '{quiz.Title}' deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting quiz {QuizUuid} for user {UserId}.", itemUuid, currentUser.Id);
            TempData["ErrorMessage"] = "An error occurred while deleting the quiz.";
        }

        Course = course;

        return RedirectToPage(new { courseUuid = CourseUuid });
    }
}