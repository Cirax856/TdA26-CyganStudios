using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("/api/courses/{courseId}/quizzes")]
public sealed class QuizzesController : ControllerBase
{
    private readonly AppDbContext _appDb;

    public QuizzesController(AppDbContext appDb)
    {
        _appDb = appDb;
    }

    [HttpGet]
    public async Task<Ok<IEnumerable<Quiz>>> GetQuizzes(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        return TypedResults.Ok((IEnumerable<Quiz>)_appDb.Quizzes
            .Where(quiz => quiz.CourseId == courseId));
    }

    [HttpPost]
    public async Task<Results<Created<Quiz>, NotFound>> CreateQuiz([FromRoute] Guid courseId, [FromBody] Quiz quiz)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var course = await _appDb.Courses
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return TypedResults.NotFound();
        }

        var dbQuiz = quiz.ToQuiz(courseId);

        _appDb.Quizzes.Add(dbQuiz);
        await _appDb.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/courses/{courseId}/quizzes/{dbQuiz.Uuid}", Quiz.FromQuiz(dbQuiz));
    }

    [HttpGet("{quizId}")]
    public async Task<Results<Ok<Quiz>, NotFound>> GetQuiz(Guid courseId, Guid quizId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var quiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(quiz => quiz.Uuid == quizId && quiz.CourseId == courseId, cancellationToken);

        if (quiz is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(Quiz.FromQuiz(quiz));
    }

    [HttpPut("{quizId}")]
    public async Task<Results<Ok<Quiz>, NotFound>> UpdateQuiz([FromRoute] Guid courseId, [FromRoute] Guid quizId, [FromBody] Quiz quiz)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var dbQuiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(quiz => quiz.Uuid == quizId && quiz.CourseId == courseId, cancellationToken);

        if (dbQuiz is null)
        {
            return TypedResults.NotFound();
        }

        quiz.AssignTo(dbQuiz);

        await _appDb.SaveChangesAsync();

        return TypedResults.Ok(Quiz.FromQuiz(dbQuiz));
    }

    [HttpDelete("{quizId}")]
    public async Task<Results<NoContent, NotFound>> DeleteQuiz(Guid courseId, Guid quizId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var quiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(quiz => quiz.Uuid == quizId && quiz.CourseId == courseId, cancellationToken);

        if (quiz is null)
        {
            return TypedResults.NotFound();
        }

        _appDb.Quizzes.Remove(quiz);
        await _appDb.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    [HttpPost("{quizId}/submit")]
    public async Task<Results<Ok<QuizSubmitResponse>, NotFound>> SubmitQuiz([FromRoute] Guid courseId, [FromRoute] Guid quizId, [FromBody] QuizSubmitRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var quiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(quiz => quiz.Uuid == quizId && quiz.CourseId == courseId, cancellationToken);

        if (quiz is null)
        {
            return TypedResults.NotFound();
        }

        bool[] correctPerQuestion = new bool[quiz.Questions.Count];
        int score = 0;
        int maxScore = 0;

        int index = 0;
        foreach (var answer in request.Answers)
        {
            int questionIndex;
            if (answer.Uuid is { } uuid)
            {
                questionIndex = -1;

                for (int i = 0; i < quiz.Questions.Count; i++)
                {
                    if (quiz.Questions[i].Uuid == uuid)
                    {
                        questionIndex = i;
                        break;
                    }
                }

                if (questionIndex is -1)
                {
                    questionIndex = index;
                }
            }
            else
            {
                questionIndex = index;
            }

            var question = quiz.Questions[questionIndex];
            if (question.IsMultiChoice)
            {
                maxScore += question.CorrectIndices.Count();

                int correctCout = 0;
                foreach (var item in question.CorrectIndices.Order().Zip((answer.SelectedIndices ?? (IEnumerable<int>)[answer.SelectedIndex ?? 0]).Order()))
                {
                    if (item.First == item.Second)
                    {
                        correctCout++;
                    }
                }

                throw new NotImplementedException();
            }
            else
            {
                maxScore++;

                var choice = answer.SelectedIndex ?? answer.SelectedIndices?.FirstOrDefault();

                if (choice is not null && question.CorrectIndices.First() == choice.Value)
                {
                    correctPerQuestion[index] = true;
                    score++;
                }
            }

            index++;
        }

        throw new NotImplementedException();
    }
}
