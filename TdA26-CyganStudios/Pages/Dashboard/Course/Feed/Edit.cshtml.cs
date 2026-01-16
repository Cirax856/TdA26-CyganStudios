using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Dashboard.Course.Feed;

[Authorize(Roles = "lecturer")]
public class EditPostModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly FeedManager _feedManager;
    private readonly ILogger<EditPostModel> _logger;

    public EditPostModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, FeedManager feedManager, ILogger<EditPostModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _feedManager = feedManager;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    [BindProperty(Name = "postUuid", SupportsGet = true)]
    public Guid PostUuid { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Text")]
        [StringLength(256, MinimumLength = 2)]
        public string Text { get; set; } = null!;
    }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var course = await _appDb.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        if (course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        var post = await _appDb.FeedItems
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Uuid == PostUuid && post.CourseId == CourseUuid, cancellationToken);

        if (post is null)
        {
            return NotFound();
        }

        if (post.Type is not DbFeedItemType.Manual)
        {
            return BadRequest();
        }

        Input = new InputModel()
        {
            Text = post.Message,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser is null)
            {
                ModelState.AddModelError(string.Empty, "Unknown error.");
                return Page();
            }

            var course = await _appDb.Courses
                 .AsNoTracking()
                 .FirstOrDefaultAsync(course => course.Uuid == CourseUuid, cancellationToken);

            if (course is null)
            {
                return NotFound();
            }

            if (course.LecturerId != currentUser.Id)
            {
                return Redirect("/");
            }

            var post = await _appDb.FeedItems
                .FirstOrDefaultAsync(post => post.Uuid == PostUuid && post.CourseId == CourseUuid, cancellationToken);

            if (post is null)
            {
                return NotFound();
            }

            if (post.Type is not DbFeedItemType.Manual)
            {
                return BadRequest();
            }

            post.Message = Input.Text;
            post.Edited = true;
            post.UpdatedAtDT = DateTimeOffset.Now;
            await _appDb.SaveChangesAsync(cancellationToken);

            return RedirectToPage("/Dashboard/Course/Index", new { courseUuid = CourseUuid });
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
