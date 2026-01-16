using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]
public class MaterialEditModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly HttpClient _httpClient;
    private readonly FeedManager _feedManager;
    private readonly ILogger<MaterialEditModel> _logger;

    public MaterialEditModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, IHttpClientFactory httpClientFactory, FeedManager feedManager, ILogger<MaterialEditModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _httpClient = httpClientFactory.CreateClient("course_material_verify");
        _feedManager = feedManager;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

    [BindProperty(Name = "materialUuid", SupportsGet = true)]
    public Guid MaterialUuid { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
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

        var material = await _appDb.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(material => material.Uuid == MaterialUuid, cancellationToken);

        if (material is null)
        {
            return NotFound();
        }

        if (material is DbUrlMaterial)
        {
            return RedirectToPage("/Dashboard/Course/EditUrlMaterial", new { courseUuid = CourseUuid, materialUuid = MaterialUuid });
        }
        else if (material is DbFileMaterial)
        {
            return RedirectToPage("/Dashboard/Course/EditFileMaterial", new { courseUuid = CourseUuid, materialUuid = MaterialUuid });
        }

        return Redirect("/");
    }
}
