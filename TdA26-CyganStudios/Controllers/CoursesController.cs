using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("/api/courses")]
public sealed class CoursesController : ControllerBase
{
    private readonly AppDbContext _appDb;

    public CoursesController(AppDbContext appDb)
    {
        _appDb = appDb;
    }

    [HttpGet]
    public IEnumerable<CourseSummary> GetCourses()
    {
        return _appDb.Courses.Select(CourseSummary.FromCourse);
    }

    [HttpPost]
    public async Task<CourseSummary> CreateCourse([FromBody] CourseCreateRequest request)
    {
        Debug.Assert(request is not null);

        var cancellationToken = HttpContext.RequestAborted;

        var lecturer = await _appDb.Users.FirstAsync(user => user.UserName == DataSeeder.DefaultUserName, cancellationToken);

        var now = DateTime.UtcNow;

        var course = new DbCourse(lecturer, request.Name, request.Description, now);
        await _appDb.Courses.AddAsync(course, cancellationToken);
        await _appDb.SaveChangesAsync(cancellationToken);

        return CourseSummary.FromCourse(course);
    }

    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourse(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .Include(course => course.Quizzes)
            .Include(course => course.FeedItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return NotFound(new ErrorResponse("The requested resource was not found."));
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return Ok(CourseDetail.FromCourse(course, baseUrl));
    }

    [HttpPut("{courseId}")]
    public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] CourseUpdateRequest request)
    {
        Debug.Assert(request is not null);

        var cancellationToken = HttpContext.RequestAborted;

        var course = await _appDb.Courses.FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return NotFound(new ErrorResponse("The requested resource was not found."));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            course.Name = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            course.Description = request.Description;
        }

        await _appDb.SaveChangesAsync(cancellationToken);

        return Ok(CourseSummary.FromCourse(course));
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> DeleteCourse(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var course = await _appDb.Courses.FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return NotFound(new ErrorResponse("The requested resource was not found."));
        }

        _appDb.Courses.Remove(course);

        await _appDb.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
