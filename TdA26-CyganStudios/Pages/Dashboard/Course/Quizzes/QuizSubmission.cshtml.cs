using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Pages.Courses;

namespace TdA26_CyganStudios.Pages.Dashboard.Course.Quizzes;

public class QuizSubmissionModel : PageModel
{
    private readonly AppDbContext _appDb;
    private readonly ILogger<QuizSubmissionModel> _logger;

    public QuizSubmissionModel(AppDbContext appDb, ILogger<QuizSubmissionModel> logger)
    {
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    [BindProperty(Name = "quizUuid", SupportsGet = true)]
    public Guid QuizUuid { get; set; }

    [BindProperty(Name = "submissionUuid", SupportsGet = true)]
    public Guid SubmissionUuid { get; set; }

    public DbQuiz Quiz { get; private set; }

    public DbQuizSubmision Submission { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var quiz = await _appDb.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(quiz => quiz.Uuid == QuizUuid && quiz.CourseId == CourseUuid);

        if (quiz is null)
        {
            return NotFound();
        }

        Quiz = quiz;

        var submission = await _appDb.QuizSubmisions
            .AsNoTracking()
            .FirstOrDefaultAsync(submission => submission.Uuid == SubmissionUuid && submission.QuizId == QuizUuid);

        if (submission is null)
        {
            return NotFound();
        }

        Submission = submission;

        return Page();
    }

    public bool IsOptionSelected(int questionIndex, int optionIndex)
    {
        var answer = Submission.Answers[questionIndex];
        return answer?.SelectedIndices?.Contains(optionIndex) == true;
    }
}
