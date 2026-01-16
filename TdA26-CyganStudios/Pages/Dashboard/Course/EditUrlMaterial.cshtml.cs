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
public class EditUrlMaterialModel : PageModel
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly HttpClient _httpClient;
    private readonly FeedManager _feedManager;
    private readonly ILogger<EditUrlMaterialModel> _logger;

    public EditUrlMaterialModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, IHttpClientFactory httpClientFactory, FeedManager feedManager, ILogger<EditUrlMaterialModel> logger)
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

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Required]
        [Display(Name = "Url")]
        public string Url { get; set; } = null!;

        [Display(Name = "Skip URL verification")]
        public bool SkipUrlVerification { get; set; } = false;
    }

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

        var material = await _appDb.UrlMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(material => material.Uuid == MaterialUuid, cancellationToken);

        if (material is null)
        {
            return NotFound();
        }

        Input = new InputModel()
        {
            Name = material.Name,
            Description = material.Description ?? string.Empty,
            Url = material.Url,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser is null)
            {
                ModelState.AddModelError(string.Empty, "Unknown error.");
                return Page();
            }

            Uri materialUrl;
            try
            {
                materialUrl = new Uri(Input.Url.Contains("://", StringComparison.Ordinal) ? Input.Url : "https://" + Input.Url);
            }
            catch (UriFormatException)
            {
                ModelState.AddModelError(string.Empty, "Material url is invalid.");
                return Page();
            }

            if (!materialUrl.IsAbsoluteUri)
            {
                ModelState.AddModelError(string.Empty, "Material url must be absolute.");
                return Page();
            }

            if (!Input.SkipUrlVerification)
            {
                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.GetAsync(materialUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    ModelState.AddModelError(string.Empty, "Could not verify material url: timeout.");
                    return Page();
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Could not verify material url.");
                    return Page();
                }

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Could not verify material url: invalid status code.");
                    return Page();
                }
            }

            var course = await _appDb.Courses
                .Include(course => course.Materials)
                .FirstOrDefaultAsync(course => course.Uuid == CourseUuid, cancellationToken);

            if (course is null)
            {
                return NotFound();
            }

            var material = await _appDb.UrlMaterials
                .FirstOrDefaultAsync(material => material.Uuid == MaterialUuid, cancellationToken);

            if (material is null)
            {
                return NotFound();
            }

            material.Name = Input.Name;
            material.Description = Input.Description;
            material.Url = materialUrl.ToString();

            await _appDb.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Material updated.");
            return RedirectToPage("/Dashboard/Course/Index", new { courseUuid = CourseUuid });
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
