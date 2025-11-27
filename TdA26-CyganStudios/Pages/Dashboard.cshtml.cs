using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages;

[Authorize(Roles = "lecturer")]
public class DashboardModel : PageModel
{
    private const int ItemsPerPage = 10;

    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<DashboardModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _logger = logger;
    }

    public IList<DbCourse> Courses { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public async Task OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            ModelState.AddModelError(string.Empty, "Unknown error.");
            return;
        }

        IQueryable<DbCourse> query = _appDb.Courses
            .Include(course => course.Lecturer)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(SearchString))
        {
            query = query.Where(course => EF.Functions.Like(course.Name, $"%{SearchString}%") ||
                (course.Description != null && EF.Functions.Like(course.Description, $"%{SearchString}%")));
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync();

        TotalPages = (int)Math.Ceiling(totalItems / (double)ItemsPerPage);

        if (PageIndex < 1)
        {
            PageIndex = 1;
        }
        else if (PageIndex > TotalPages && TotalPages > 0)
        {
            PageIndex = TotalPages;
        }

        Courses = await query
            .Skip((PageIndex - 1) * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid courseUuid)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null)
        {
            return Challenge();
        }

        var course = await _appDb.Courses.FirstOrDefaultAsync(c => c.Uuid == courseUuid);

        if (course is null)
        {
            _logger.LogWarning("Delete attempt failed: Course {CourseUuid} not found.", courseUuid);
            return NotFound();
        }

        if (course.LecturerId != currentUser.Id)
        {
            _logger.LogWarning("Delete attempt FORBIDDEN: User {UserId} does not own course {CourseUuid} (Owner: {OwnerId}).", currentUser.Id, courseUuid, course.LecturerId);
            return Forbid();
        }

        try
        {
            _appDb.Courses.Remove(course);
            await _appDb.SaveChangesAsync();

            _logger.LogInformation("Course {CourseUuid} deleted successfully by user {UserId}.", courseUuid, currentUser.Id);

            TempData["SuccessMessage"] = $"Course '{course.Name}' deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseUuid} for user {UserId}.", courseUuid, currentUser.Id);
            TempData["ErrorMessage"] = "An error occurred while deleting the course.";
        }

        return RedirectToPage();
    }
}
