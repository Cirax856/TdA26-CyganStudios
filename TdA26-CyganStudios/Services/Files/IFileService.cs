namespace TdA26_CyganStudios.Services.Files;

public interface IFileService
{
    Task<Guid> CreateAsync(Stream stream, CancellationToken cancellationToken = default);
    
    Task<Stream?> GetAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}