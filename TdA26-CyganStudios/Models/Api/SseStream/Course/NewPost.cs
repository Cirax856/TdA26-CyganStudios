namespace TdA26_CyganStudios.Models.Api.SseStream.Course;

public sealed record NewPost(Guid Uuid, DateTime CreatedAt, bool IsSystemMessage, string Message);