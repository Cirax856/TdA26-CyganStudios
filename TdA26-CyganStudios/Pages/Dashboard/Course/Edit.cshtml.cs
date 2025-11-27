using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]

public class EditModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<CourseNewModel> _logger;

    public EditModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<CourseNewModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

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

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        Input = new InputModel();
        var course = await _appDb.Courses.FirstOrDefaultAsync(course => course.Uuid == CourseUuid);
        if (course is null)
        {
            return NotFound();
        }

        if (course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        Input.Name = course.Name;
        Input.Description = course.Description;

        return Page();
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

            var course = _appDb.Courses.FirstOrDefault(course => course.Uuid == CourseUuid);
            course.Name = Input.Name;
            course.Description = Input.Description;
            await _appDb.SaveChangesAsync();

            _logger.LogInformation("Course successfully edddited.");
            return RedirectToPage("/Dashboard/Course/Index", new { courseUuid = CourseUuid });
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
