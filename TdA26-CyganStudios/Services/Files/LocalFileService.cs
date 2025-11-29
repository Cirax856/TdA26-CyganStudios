
namespace TdA26_CyganStudios.Services.Files;

internal sealed class LocalFileService : IFileService
{
    private readonly string _storagePath;

    public LocalFileService()
    {
        _storagePath = Path.GetFullPath(Path.Combine("storage", "materials"))!;
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<Guid> CreateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var fileGuid = Guid.CreateVersion7();

        using (var fs = File.OpenWrite(GetFilePath(fileGuid)))
        {
            await stream.CopyToAsync(fs, cancellationToken);
        }

        return fileGuid;
    }

    public async Task<Stream?> GetAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileId);

        if (!File.Exists(filePath))
        {
            return null;
        }

        return File.OpenRead(filePath);
    }

    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileId);

        bool exists = File.Exists(filePath);

        File.Delete(filePath);

        return exists;
    }

    private string GetFilePath(Guid id)
        => Path.Combine(_storagePath, id.ToString("N"));
}