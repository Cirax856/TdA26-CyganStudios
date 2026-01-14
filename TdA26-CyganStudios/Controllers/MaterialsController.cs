using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("/api/courses/{courseId}/materials")]
public sealed class MaterialsController : ControllerBase
{
    private readonly AppDbContext _appDb;
    private readonly FeedManager _feedManager;
    private readonly IFileService _fileService;
    private readonly FileExtensionContentTypeProvider _fileExtensionTypeProvider;
    private readonly MimeTypeToExtensionProvider _mimeTypeToExtension;

    public MaterialsController(AppDbContext appDb, FeedManager feedManager, IFileService fileService, FileExtensionContentTypeProvider fileExtensionTypeProvider, MimeTypeToExtensionProvider mimeTypeToExtension)
    {
        _appDb = appDb;
        _feedManager = feedManager;
        _fileService = fileService;
        _fileExtensionTypeProvider = fileExtensionTypeProvider;
        _mimeTypeToExtension = mimeTypeToExtension;
    }

    [HttpGet]
    public async Task<Results<Ok<IEnumerable<Material>>, NotFound>> GetMaterials(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return TypedResults.NotFound();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return TypedResults.Ok(course.Materials.OrderByDescending(material => material.CreatedAt).Select(material => Material.FromMaterial(material, baseUrl)));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<Results<Created<Material>, BadRequest, BadRequest<object>, NotFound>> CreateFileMaterial([FromRoute] Guid courseId, [FromForm] FileMaterialCreateRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var file = request.File;

        if (file is null)
        {
            return TypedResults.BadRequest();
        }

        // Validate extension
        if (!DbFileMaterial.IsExtensionAllowed(Path.GetExtension(file.FileName)))
        {
            return TypedResults.BadRequest<object>(new { error = "Invalid file extension", });
        }

        // Validate size
        if (file.Length > DbFileMaterial.MaxFileSize)
        {
            return TypedResults.BadRequest<object>(new { error = "File too large", });
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return TypedResults.NotFound();
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

        var material = new DbFileMaterial()
        {
            Name = request.Name,
            Description = request.Description,
            FileUuid = fileId,
            MimeType = mimeType,
            SizeInBytes = file.Length,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        course.Materials.Add(material);
        await _appDb.SaveChangesAsync(cancellationToken);

        await _feedManager.NewMaterialCreatedAsync(material);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return TypedResults.Created($"/api/courses/{courseId}/materials/{material.Uuid}", Material.FromMaterial(material, baseUrl));
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<Results<Created<Material>, BadRequest<string>, NotFound>> CreateUrlMaterial([FromRoute] Guid courseId, [FromBody] UrlMaterialCreateRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        Uri materialUrl;
        try
        {
            materialUrl = new Uri(request.Url.Contains("://", StringComparison.Ordinal) ? request.Url : "https://" + request.Url);
        }
        catch (UriFormatException)
        {
            return TypedResults.BadRequest("Material url is invalid.");
        }

        if (!materialUrl.IsAbsoluteUri)
        {
            return TypedResults.BadRequest("Material url must be absolute.");
        }

        var course = await _appDb.Courses
            .Include(course => course.Materials)
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return TypedResults.NotFound();
        }

        var material = new DbUrlMaterial()
        {
            Name = request.Name,
            Description = request.Description,
            Url = materialUrl.ToString(),
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        course.Materials.Add(material);
        await _appDb.SaveChangesAsync(cancellationToken);
        
        await _feedManager.NewMaterialCreatedAsync(material);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return TypedResults.Created($"/api/courses/{courseId}/materials/{material.Uuid}", Material.FromMaterial(material, baseUrl));
    }

    [HttpPut("{materialId}")]
    [Consumes("multipart/form-data")]
    public async Task<Results<Ok<Material>, BadRequest, NotFound>> UpdateFileMaterial([FromRoute] Guid courseId, [FromRoute] Guid materialId, [FromForm] FileMaterialUpdateRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var material = await _appDb.FileMaterials
            .FirstOrDefaultAsync(material => material.CourseId == courseId && material.Uuid == materialId, cancellationToken);

        if (material is null)
        {
            return TypedResults.NotFound();
        }

        var file = request.File;
        if (file is not null)
        {
            // Validate extension
            if (!DbFileMaterial.IsExtensionAllowed(Path.GetExtension(file.FileName)))
            {
                return TypedResults.BadRequest();
            }

            // Validate size
            if (file.Length > DbFileMaterial.MaxFileSize)
            {
                return TypedResults.BadRequest();
            }

            // delete old file
            await _fileService.DeleteAsync(material.FileUuid);

            Guid fileId;
            using (var fileStream = file.OpenReadStream())
            {
                fileId = await _fileService.CreateAsync(fileStream);
            }

            if (!_fileExtensionTypeProvider.TryGetContentType(file.FileName, out var mimeType))
            {
                mimeType = "application/octet-stream"; // fallback
            }

            material.FileUuid = fileId;
            material.MimeType = mimeType;
            material.SizeInBytes = file.Length;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            material.Name = request.Name;
        }

        if (request.Description is not null)
        {
            material.Description = request.Description;
        }

        await _appDb.SaveChangesAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return TypedResults.Ok(Material.FromMaterial(material, baseUrl));
    }

    [HttpPut("{materialId}")]
    [Consumes("application/json")]
    public async Task<Results<Ok<Material>, BadRequest<string>, NotFound>> UpdateFileOrUrlMaterialMetadata([FromRoute] Guid courseId, [FromRoute] Guid materialId, [FromBody] UrlMaterialUpdateRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var material = await _appDb.Materials
            .FirstOrDefaultAsync(material => material.CourseId == courseId && material.Uuid == materialId, cancellationToken);

        if (material is null)
        {
            return TypedResults.NotFound();
        }

        if (request.Url is not null && material is DbUrlMaterial urlMaterial)
        {
            Uri materialUrl;
            try
            {
                materialUrl = new Uri(request.Url.Contains("://", StringComparison.Ordinal) ? request.Url : "https://" + request.Url);
            }
            catch (UriFormatException)
            {
                return TypedResults.BadRequest("Material url is invalid.");
            }

            if (!materialUrl.IsAbsoluteUri)
            {
                return TypedResults.BadRequest("Material url must be absolute.");
            }

            urlMaterial.Url = materialUrl.ToString();
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            material.Name = request.Name;
        }

        if (request.Description is not null)
        {
            material.Description = request.Description;
        }

        await _appDb.SaveChangesAsync(cancellationToken);

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return TypedResults.Ok(Material.FromMaterial(material, baseUrl));
    }

    [HttpDelete("{materialId}")]
    public async Task<IActionResult> DeleteMaterial(Guid courseId, Guid materialId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var material = await _appDb.Materials
            .FirstOrDefaultAsync(material => material.CourseId == courseId && material.Uuid == materialId, cancellationToken);

        if (material is null)
        {
            return NotFound();
        }

        _appDb.Materials.Remove(material);
        await _appDb.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("{materialId}/download")]
    public async Task<IActionResult> DownloadMaterial(Guid courseId, Guid materialId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var material = await _appDb.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Uuid == materialId && m.Course.Uuid == courseId, cancellationToken);

        if (material is null)
        {
            return NotFound();
        }

        if (material is not DbFileMaterial fileMaterial)
        {
            return BadRequest();
        }

        var stream = await _fileService.GetAsync(fileMaterial.FileUuid, cancellationToken);

        if (stream is null)
        {
            return NotFound();
        }

        return new FileStreamResult(stream, fileMaterial.MimeType)
        {
            FileDownloadName = $"{material.Name}{_mimeTypeToExtension.GetExtension(fileMaterial.MimeType)}", // dot included in extension
        };
    }

    [HttpGet("{materialId}/preview")]
    public async Task<IActionResult> PreviewMaterial(Guid courseId, Guid materialId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var material = await _appDb.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Uuid == materialId && m.Course.Uuid == courseId, cancellationToken);

        if (material is null)
        {
            return NotFound();
        }

        if (material is not DbFileMaterial fileMaterial)
        {
            return BadRequest();
        }

        var stream = await _fileService.GetAsync(fileMaterial.FileUuid, cancellationToken);

        if (stream is null)
        {
            return NotFound();
        }

        Response.Headers.ContentDisposition = "inline";

        return new FileStreamResult(stream, fileMaterial.MimeType);
    }
}