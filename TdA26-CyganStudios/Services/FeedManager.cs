using System.Text.Json;
using System.Text.Json.Serialization;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Api.SseStream.Course;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Services;

public sealed class FeedManager
{
    private static readonly JsonSerializerOptions feedSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AppDbContext _appDb;
    private readonly SseConnectionManager _sseConnectionManager;

    public FeedManager(AppDbContext appDb, SseConnectionManager sseConnectionManager)
    {
        _appDb = appDb;
        _sseConnectionManager = sseConnectionManager;
    }

    public async Task NewCoursePostAsync(Guid courseId, string message, FeedItemType messageType)
    {
        var feedItem = new DbFeedItem()
        {
            CourseId = courseId,
            Type = messageType switch { FeedItemType.Manual => DbFeedItemType.Manual, FeedItemType.System => DbFeedItemType.System, _ => default },
            Message = message,
            Edited = false,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        _appDb.FeedItems.Add(feedItem);
        await _appDb.SaveChangesAsync();

        await _sseConnectionManager.BroadcastCourseAsync(courseId, "new_post", JsonSerializer.Serialize(new NewPost(feedItem.Uuid, feedItem.Message), feedSerializerOptions));
    }
}