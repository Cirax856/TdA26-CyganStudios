using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Dashboard.Course.Feed;

[Authorize(Roles = "lecturer")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public DbFeedItem[] FeedItems { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var course = await _appDb.Courses
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

        FeedItems = await _appDb.FeedItems
            .Where(item => item.CourseId == CourseUuid)
            .AsNoTracking()
            .ToArrayAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemUuid, string type)
    {
        if (type is not "feed-item")
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

        return RedirectToPage(new { courseUuid = CourseUuid });
    }
}