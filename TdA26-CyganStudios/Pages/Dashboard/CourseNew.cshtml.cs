using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Dashboard;

[Authorize(Roles = "lecturer")]
public class CourseNewModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<CourseNewModel> _logger;

    public CourseNewModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<CourseNewModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser is null)
            {
                ModelState.AddModelError(string.Empty, "Unknown error.");
                return Page();
            }

            var course = new DbCourse(currentUser, Input.Name, Input.Description, DateTimeOffset.UtcNow);
            _appDb.Courses.Add(course);
            await _appDb.SaveChangesAsync();

            _logger.LogInformation("Course created.");
            return LocalRedirect("~/dashboard");
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
