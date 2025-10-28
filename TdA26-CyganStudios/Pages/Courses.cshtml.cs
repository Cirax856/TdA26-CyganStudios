using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages;

public class CoursesModel : PageModel
{
    private const int ItemsPerPage = 10;

    private readonly AppDbContext _appDb;
    private readonly ILogger<CoursesModel> _logger;

    public CoursesModel(AppDbContext appDb, ILogger<CoursesModel> logger)
    {
        _appDb = appDb;
        _logger = logger;
    }

    public IList<DbCourse> Courses { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public async Task OnGetAsync()
    {
        IQueryable<DbCourse> query = _appDb.Courses
            .Include(course => course.Lecturer)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(SearchString))
        {
            query = query.Where(course => EF.Functions.Like(course.Name, $"%{SearchString}%") ||
                (course.Description != null && EF.Functions.Like(course.Description, $"%{SearchString}%")));
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync();

        TotalPages = (int)Math.Ceiling(totalItems / (double)ItemsPerPage);

        if (PageIndex < 1)
        {
            PageIndex = 1;
        }
        else if (PageIndex > TotalPages && TotalPages > 0)
        {
            PageIndex = TotalPages;
        }

        Courses = await query
            .Skip((PageIndex - 1) * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToListAsync();
    }
}
