using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Api;

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
    public async Task<Results<Ok<IEnumerable<Quiz>>, NotFound>> GetQuizzes(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var course = await _appDb.Courses
            .Include(course => course.Quizzes)
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return TypedResults.NotFound();
        }

        throw new NotImplementedException();
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

        throw new NotImplementedException();
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

        throw new NotImplementedException();
    }

    [HttpPut("{quizId}")]
    public async Task<Results<Ok<Quiz>, NotFound>> UpdateQuiz(Guid courseId, Guid quizId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var quiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(quiz => quiz.Uuid == quizId && quiz.CourseId == courseId, cancellationToken);

        if (quiz is null)
        {
            return TypedResults.NotFound();
        }

        throw new NotImplementedException();
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
    public async Task<Results<Ok, NotFound>> SubmitQuiz(Guid courseId, Guid quizId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var quiz = await _appDb.Quizzes
            .FirstOrDefaultAsync(quiz => quiz.Uuid == quizId && quiz.CourseId == courseId, cancellationToken);

        if (quiz is null)
        {
            return TypedResults.NotFound();
        }

        throw new NotImplementedException();
    }
}
