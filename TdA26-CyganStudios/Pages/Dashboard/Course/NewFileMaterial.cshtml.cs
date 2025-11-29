using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Pages.Dashboard.Course;

[Authorize(Roles = "lecturer")]
public class NewFileMaterialModel : PageModel
{
    private const long MaxFileSize = 30 * 1024 * 1024; // 30 MB

    private static readonly FrozenSet<string> AllowedExtensions = FrozenSet.Create(StringComparer.InvariantCultureIgnoreCase, ".pdf", ".docx", ".txt", ".png", ".jpg", ".jpeg", ".gif", ".mp4", ".mp3");

    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly AppDbContext _appDb;
    private readonly IFileService _fileService;
    private readonly FileExtensionContentTypeProvider _fileExtensionTypeProvider;
    private readonly ILogger<CourseNewModel> _logger;

    public NewFileMaterialModel(UserManager<IdentityUser<int>> userManager, AppDbContext appDb, IFileService fileService, FileExtensionContentTypeProvider fileExtensionTypeProvider, ILogger<CourseNewModel> logger)
    {
        _userManager = userManager;
        _appDb = appDb;
        _fileService = fileService;
        _fileExtensionTypeProvider = fileExtensionTypeProvider;
        _logger = logger;
    }

    [BindProperty(Name = "courseUuid", SupportsGet = true)]
    public Guid CourseUuid { get; set; }

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
        [Display(Name = "File")]
        public IFormFile File { get; set; } = null!;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            return RedirectToPage("/Login");
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var file = Input.File;

        // Validate extension
        if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName)))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.File)}", "Unsupported file type.");
            return Page();
        }

        // Validate size
        if (file.Length > MaxFileSize)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.File)}", "File is too large (max 30 MB).");
            return Page();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser is null)
        {
            ModelState.AddModelError(string.Empty, "Unknown error.");
            return Page();
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .FirstOrDefaultAsync(course => course.Uuid == CourseUuid, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        Guid fileId;
        using (var fileStream = file.OpenReadStream())
        {
            fileId = await _fileService.CreateAsync(fileStream, cancellationToken);
        }

        if (!_fileExtensionTypeProvider.TryGetContentType(file.FileName, out var mimeType))
        {
            mimeType = "application/octet-stream"; // fallback
        }

        course.Materials.Add(new DbFileMaterial()
        {
            Name = Input.Name,
            Description = Input.Description,
            FileUuid = fileId,
            MimeType = mimeType,
            SizeInBytes = file.Length,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        });
        await _appDb.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Material created.");
        return RedirectToPage("/Dashboard/Course/Index", new { courseUuid = CourseUuid });
    }
}
