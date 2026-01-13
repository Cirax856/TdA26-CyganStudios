using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Courses;

public class QuizzesModel : PageModel
{
    private readonly AppDbContext _appDb;
    private readonly ILogger<QuizzesModel> _logger;

    public QuizzesModel(AppDbContext appDb, ILogger<QuizzesModel> logger)
    {
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public DbCourse Course { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var course = await _appDb.Courses
            .Include(course => course.Quizzes)
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid);

        if (course is null)
        {
            return NotFound();
        }

        Course = course;

        return Page();
    }
}