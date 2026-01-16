using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Courses;

public class FeedModel : PageModel
{
    private readonly AppDbContext _appDb;
    private readonly ILogger<FeedModel> _logger;

    public FeedModel(AppDbContext appDb, ILogger<FeedModel> logger)
    {
        _appDb = appDb;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    public DbFeedItem[] FeedItems { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        FeedItems = await _appDb.FeedItems
            .Where(item => item.CourseId == CourseUuid)
            .AsNoTracking()
            .ToArrayAsync();

        return Page();
    }
}