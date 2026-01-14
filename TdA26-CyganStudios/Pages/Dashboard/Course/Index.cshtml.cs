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
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly IFileService _fileService;
    private readonly ILogger<CourseNewModel> _logger;

    public IndexModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, IFileService fileService, ILogger<CourseNewModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _fileService = fileService;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public DbCourse Course { get; set; }

    public DbFeedItem[] FeedItems { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .Include(course => course.Quizzes)
            .ThenInclude(quiz => quiz.Submisions)
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

        FeedItems = await _appDb.FeedItems
            .Where(item => item.CourseId == CourseUuid)
            .AsNoTracking()
            .ToArrayAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemUuid, string type)
    {
        if (type is not ("material" or "quiz"))
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

        switch (type)
        {
            case "material":
                {
                    var material = await _appDb.Materials
                        .FirstOrDefaultAsync(material => material.CourseId == CourseUuid && material.Uuid == itemUuid);

                    if (material is null)
                    {
                        return NotFound();
                    }

                    try
                    {
                        course.Materials.Remove(material);
                        await _appDb.SaveChangesAsync();

                        if (material is DbFileMaterial fileMaterial)
                        {
                            await _fileService.DeleteAsync(fileMaterial.FileUuid);
                        }

                        _logger.LogInformation("Material deleted.");

                        TempData["SuccessMessage"] = $"Material '{material.Name}' deleted successfully.";
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "Error deleting material {MaterialUuid} for user {UserId}.", itemUuid, currentUser.Id);
                        TempData["ErrorMessage"] = "An error occurred while deleting the material.";
                    }
                }

                break;
            case "quiz":
                {
                    var quiz = await _appDb.Quizzes
                                         .FirstOrDefaultAsync(quiz => quiz.CourseId == CourseUuid && quiz.Uuid == itemUuid);

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
                        _logger.LogError(ex, "Error deleting quiz {QuizlUuid} for user {UserId}.", itemUuid, currentUser.Id);
                        TempData["ErrorMessage"] = "An error occurred while deleting the quiz.";
                    }
                }

                break;
        }

        return RedirectToPage(new { courseUuid = CourseUuid });
    }
}
