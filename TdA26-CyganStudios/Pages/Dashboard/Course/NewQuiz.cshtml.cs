using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]
public class NewQuizModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly FeedManager _feedManager;
    private readonly ILogger<NewQuizModel> _logger;

    public NewQuizModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, FeedManager feedManager, ILogger<NewQuizModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _feedManager = feedManager;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    [BindProperty]
    public QuizInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
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

        Input.Questions.Add(new QuestionInput
        {
            Options = new List<OptionInput> { new() { IsCorrect = true, }, new() }
        });

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

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

        var newQuiz = new DbQuiz
        {
            CourseId = CourseUuid,
            Title = Input.Title,
            AttemptsCount = Input.AttemptsCount is null or < 1 ? null : Input.AttemptsCount,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Questions = new List<DbQuestion>(),
        };

        foreach (var question in Input.Questions)
        {
            var optionStrings = question.Options
                .Select(o => o.OptionText)
                .ToArray();

            var correctIndices = question.Options
                .Select((opt, index) => new { opt, index })
                .Where(x => x.opt.IsCorrect)
                .Select(x => x.index)
                .ToArray();

            var dbQuestion = new DbQuestion(
                Uuid: Guid.NewGuid(),
                Question: question.QuestionText,
                Options: optionStrings,
                IsMultiChoice: question.IsMultiChoice,
                CorrectIndices: correctIndices
            );

            newQuiz.Questions.Add(dbQuestion);
        }

        _appDb.Quizzes.Add(newQuiz);
        await _appDb.SaveChangesAsync(cancellationToken);

        await _feedManager.NewQuizCreatedAsync(newQuiz);

        _logger.LogInformation("Quiz created.");
        return RedirectToPage("/Dashboard/Course/Index", new { courseUuid = CourseUuid });
    }

    public class QuizInputModel
    {
        [Required(ErrorMessage = "Title text is required")]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; }

        [Range(1, 100)]
        public int? AttemptsCount { get; set; }

        public List<QuestionInput> Questions { get; set; } = new();
    }

    public class QuestionInput
    {
        [Required(ErrorMessage = "Question text is required")]
        public string QuestionText { get; set; }
        public bool IsMultiChoice { get; set; }
        public List<OptionInput> Options { get; set; } = new();
    }

    public class OptionInput
    {
        [Required(ErrorMessage = "Option text is required")]
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
