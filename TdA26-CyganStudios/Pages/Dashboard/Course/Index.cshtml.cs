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

        // automatically publish if the scheduled time has arrived; this
        // cases covers users hitting the page before the background service
        // runs. we update the state and clear the schedule field.
        if (course.State is CourseState.Draft &&
            course.ScheduledPublishAtDT.HasValue &&
            course.ScheduledPublishAtDT <= DateTimeOffset.UtcNow)
        {
            course.State = CourseState.Published;
            course.ScheduledPublishAt = null;
            await _appDb.SaveChangesAsync();
            TempData["SuccessMessage"] = "Scheduled publish executed.";
        }

        Course = course;

        FeedItems = await _appDb.FeedItems
            .Where(item => item.CourseId == CourseUuid)
            .AsNoTracking()
            .ToArrayAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostSetStateAsync(string action)
    {
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

        switch (action)
        {
            case "publish":
                course.State = CourseState.Published;
                course.ScheduledPublishAt = null; // clear any existing schedule
                break;
            case "archive":
                course.State = CourseState.Archived;
                break;
            case "pause":
                course.State = CourseState.Paused;
                break;
            case "draft":
                course.State = CourseState.Draft;
                break;
            case "unarchive":
            case "unpause":
                course.State = CourseState.Published;
                break;
            default:
                return BadRequest();
        }

        try
        {
            await _appDb.SaveChangesAsync();
            TempData["SuccessMessage"] = "Course state updated.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error changing course state {CourseUuid} for user {UserId}.", CourseUuid, currentUser.Id);
            TempData["ErrorMessage"] = "An error occurred while updating the course state.";
        }

        return RedirectToPage(new { courseUuid = CourseUuid });
    }

    // schedule the draft course to be published at a later time. if the
    // requested time is in the past we immediately publish instead.
    public async Task<IActionResult> OnPostSchedulePublishAsync(DateTime? publishAt)
    {
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

        if (course.State != CourseState.Draft)
        {
            TempData["ErrorMessage"] = "Only draft courses can be scheduled.";
            return RedirectToPage(new { courseUuid = CourseUuid });
        }

        if (!publishAt.HasValue)
        {
            TempData["ErrorMessage"] = "Please provide a date and time for scheduling.";
            return RedirectToPage(new { courseUuid = CourseUuid });
        }

        var at = publishAt.Value.ToUniversalTime();
        if (at <= DateTimeOffset.UtcNow)
        {
            // publish immediately
            course.State = CourseState.Published;
            course.ScheduledPublishAt = null;
            TempData["SuccessMessage"] = "Course published immediately because the given time was not in the future.";
        }
        else
        {
            course.ScheduledPublishAtDT = at;
            TempData["SuccessMessage"] = $"Publish scheduled for {at:yyyy-MM-dd HH:mm} UTC.";
        }

        try
        {
            await _appDb.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error scheduling publish for course {CourseUuid} by user {UserId}.", CourseUuid, currentUser.Id);
            TempData["ErrorMessage"] = "An error occurred while scheduling the publish.";
        }

        return RedirectToPage(new { courseUuid = CourseUuid });
    }

    public async Task<IActionResult> OnPostClearScheduleAsync()
    {
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

        if (course.ScheduledPublishAt.HasValue)
        {
            course.ScheduledPublishAt = null;
            try
            {
                await _appDb.SaveChangesAsync();
                TempData["SuccessMessage"] = "Scheduled publish cancelled.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error clearing scheduled publish for course {CourseUuid} by user {UserId}.", CourseUuid, currentUser.Id);
                TempData["ErrorMessage"] = "Could not cancel scheduled publish.";
            }
        }

        return RedirectToPage(new { courseUuid = CourseUuid });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemUuid, string type)
    {
        if (type is not ("material" or "quiz" or "feed-item"))
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
            case "feed-item":
                {
                    var feedItem = await _appDb.FeedItems
                        .FirstOrDefaultAsync(feedItem => feedItem.CourseId == CourseUuid && feedItem.Uuid == itemUuid);

                    if (feedItem is null)
                    {
                        return NotFound();
                    }

                    try
                    {
                        course.FeedItems.Remove(feedItem);
                        await _appDb.SaveChangesAsync();

                        _logger.LogInformation("Feed item deleted.");

                        TempData["SuccessMessage"] = $"Post deleted successfully.";
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "Error deleting post {PostUuid} for user {UserId}.", itemUuid, currentUser.Id);
                        TempData["ErrorMessage"] = "An error occurred while deleting the post.";
                    }
                }

                break;
        }

        return RedirectToPage(new { courseUuid = CourseUuid });
    }
}
