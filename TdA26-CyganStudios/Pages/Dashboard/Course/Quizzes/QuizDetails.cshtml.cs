using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Dashboard.Course.Quizzes;

[Authorize(Roles = "lecturer")]
public class QuizDetails : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly ILogger<QuizDetails> _logger;

    public QuizDetails(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, ILogger<QuizDetails> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    [BindProperty(Name = "quizUuid", SupportsGet = true)]
    public Guid QuizUuid { get; set; }

    public DbQuiz Quiz { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var quiz = await _appDb.Quizzes
            .Include(course => course.Course)
            .Include(course => course.Submisions)
            .AsNoTracking()
            .FirstOrDefaultAsync(quiz => quiz.Uuid == QuizUuid && quiz.CourseId == CourseUuid);

        if (quiz is null)
        {
            return NotFound();
        }

        if (quiz.Course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        Quiz = quiz;

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var quiz = await _appDb.Quizzes
            .Include(quiz => quiz.Course)
            .FirstOrDefaultAsync(quiz => quiz.Uuid == QuizUuid && quiz.CourseId == CourseUuid);

        if (quiz is null)
        {
            return NotFound();
        }

        if (quiz.Course.LecturerId != currentUser.Id)
        {
            return Redirect("/");
        }

        _appDb.Quizzes.Remove(quiz);
        await _appDb.SaveChangesAsync();

        return RedirectToPage("/Dashboard/Course/Index", new { courseUuid = CourseUuid });
    }
}