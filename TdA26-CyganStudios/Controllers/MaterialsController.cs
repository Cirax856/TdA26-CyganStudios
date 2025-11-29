using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services;
using TdA26_CyganStudios.Services.Files;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("/api/courses/{courseId}/materials")]
public sealed class MaterialsController : ControllerBase
{
    private readonly AppDbContext _appDb;
    private readonly IFileService _fileService;
    private readonly MimeTypeToExtensionProvider _mimeTypeToExtension;

    public MaterialsController(AppDbContext appDb, IFileService fileService, MimeTypeToExtensionProvider mimeTypeToExtension)
    {
        _appDb = appDb;
        _fileService = fileService;
        _mimeTypeToExtension = mimeTypeToExtension;
    }

    [Route("{materialId}/download")]
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
}