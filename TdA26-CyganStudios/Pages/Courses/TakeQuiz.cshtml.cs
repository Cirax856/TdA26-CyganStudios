using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Courses;

public class TakeQuizModel : PageModel
{
    private readonly AppDbContext _appDb;
    private readonly ILogger<TakeQuizModel> _logger;

    public TakeQuizModel(AppDbContext appDb, ILogger<TakeQuizModel> logger)
    {
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    [BindProperty(Name = "quizUuid", SupportsGet = true)]
    public Guid QuizUuid { get; set; }

    public DbQuiz Quiz { get; set; }

    [BindProperty]
    public List<UserAnswer> Answers { get; set; } = [];

    public int? Score { get; private set; }

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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var quiz = await _appDb.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(quiz => quiz.Uuid == QuizUuid && quiz.CourseId == CourseUuid);

        if (quiz is null)
        {
            return NotFound();
        }

        Quiz = quiz;

        int score = 0;
        var dbAnswers = new List<DbQuizAnswer>();

        for (int i = 0; i < Quiz.Questions.Count; i++)
        {
            var question = Quiz.Questions[i];
            var user = Answers.FirstOrDefault(a => a.QuestionIndex == i);

            int[] selected = user?.SelectedIndices ?? [];

            int questionScore = selected
                .Intersect(question.CorrectIndices)
                .Count();

            score += questionScore;

            dbAnswers.Add(new DbQuizAnswer(
                Uuid: null,
                SelectedIndices: selected,
                Comment: null
            ));
        }

        Score = score;

        _appDb.QuizSubmisions.Add(new DbQuizSubmision
        {
            QuizId = Quiz.Uuid,
            Answers = dbAnswers,
            Score = score,
            MaxScore = Quiz.Questions.Sum(q => q.CorrectIndices.Count()),
            SubmitedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        await _appDb.SaveChangesAsync();

        return Page();
    }

    public bool IsOptionSelected(int questionIndex, int optionIndex)
    {
        var answer = Answers.FirstOrDefault(a => a.QuestionIndex == questionIndex);
        return answer?.SelectedIndices?.Contains(optionIndex) == true;
    }

    public sealed record UserAnswer(int QuestionIndex, int[] SelectedIndices);
}