using System.Diagnostics;
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
        var now = DateTimeOffset.UtcNow;
        var feedItem = new DbFeedItem()
        {
            CourseId = courseId,
            Type = messageType switch { FeedItemType.Manual => DbFeedItemType.Manual, FeedItemType.System => DbFeedItemType.System, _ => default },
            Message = message,
            Edited = false,
            CreatedAt = now.ToUnixTimeMilliseconds(),
        };

        _appDb.FeedItems.Add(feedItem);
        await _appDb.SaveChangesAsync();

        await _sseConnectionManager.BroadcastCourseAsync(courseId, "new_post", JsonSerializer.Serialize(new NewPost(feedItem.Uuid, feedItem.CreatedAtDT.DateTime, feedItem.Type is DbFeedItemType.System, feedItem.Message), feedSerializerOptions));
    }

    public async Task NewMaterialCreatedAsync(DbMaterial material)
    {
        await NewCoursePostAsync(material.CourseId, material switch
        {
            DbUrlMaterial urlMaterial => $"New url material '{material.Name}' was created, <img src='https://www.google.com/s2/favicons?domain={new Uri(urlMaterial.Url).Host}&sz=32' alt='favicon' width='16' height='16' class='rounded' /> <a href='{urlMaterial.Url}' target=”_blank”>visit site</a>",
            DbFileMaterial fileMaterial => $"New file material '{material.Name}' was created,{(fileMaterial.IsPreviewable ? $" <a href='/api/courses/{material.CourseId}/materials/{material.Uuid}/preview' target=”_blank”>preview</a>" : "")} <a href='/api/courses/{material.CourseId}/materials/{material.Uuid}/download' target=”_blank” >download</a>",
            _ => throw new UnreachableException(),
        }, FeedItemType.System);
    }

    public async Task NewQuizCreatedAsync(DbQuiz quiz)
    {
        await NewCoursePostAsync(quiz.CourseId, $"New quiz '<a href='/courses/{quiz.CourseId}/quiz_take?quizUuid={quiz.Uuid}'>{quiz.Title}</a>' was created", FeedItemType.System);
    }
}