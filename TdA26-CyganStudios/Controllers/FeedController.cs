using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TdA26_CyganStudios.Models.Api;
using TdA26_CyganStudios.Models.Api.SseStream.Course;
using TdA26_CyganStudios.Models.Db;
using TdA26_CyganStudios.Services;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("/api/courses/{courseId}/feed")]
public sealed class FeedController : ControllerBase
{
    private static readonly JsonSerializerOptions feedSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AppDbContext _appDb;
    private readonly SseConnectionManager _sseConnectionManager;

    public FeedController(AppDbContext appDb, SseConnectionManager sseConnectionManager)
    {
        _appDb = appDb;
        _sseConnectionManager = sseConnectionManager;
    }

    [HttpGet]
    public async Task<Ok<IEnumerable<FeedItem>>> GetFeed(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        return TypedResults.Ok(((IEnumerable<DbFeedItem>)_appDb.FeedItems
            .Where(feedItem => feedItem.CourseId == courseId))
            .Select(FeedItem.FromFeedItem));
    }

    [HttpPost]
    public async Task<Results<Created<FeedItem>, NotFound>> AddFeedItem([FromRoute] Guid courseId, [FromBody] FeedCreateRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var now = DateTimeOffset.UtcNow;

        var course = await _appDb.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Uuid == courseId, cancellationToken);

        if (course is null)
        {
            return TypedResults.NotFound();
        }

        var feedItem = new DbFeedItem()
        {
            CourseId = courseId,
            Type = DbFeedItemType.Manual,
            Message = request.Message,
            Edited = false,
            CreatedAt = now.ToUnixTimeMilliseconds(),
        };

        _appDb.FeedItems.Add(feedItem);
        await _appDb.SaveChangesAsync(cancellationToken);

        await _sseConnectionManager.BroadcastCourseAsync(courseId, "new_post", JsonSerializer.Serialize(new NewPost(feedItem.Uuid, feedItem.Message), feedSerializerOptions));

        return TypedResults.Created($"/api/courses/{courseId}/feed", FeedItem.FromFeedItem(feedItem));
    }

    [HttpPut("{postId}")]
    public async Task<Results<Ok<FeedItem>, NotFound>> UpdateFeedItem([FromRoute] Guid courseId, [FromRoute] Guid postId, [FromBody] FeedUpdateRequest request)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var now = DateTimeOffset.UtcNow;

        var feedItem = await _appDb.FeedItems
            .FirstOrDefaultAsync(feedItem => feedItem.Uuid == postId && feedItem.CourseId == courseId, cancellationToken);

        if (feedItem is null)
        {
            return TypedResults.NotFound();
        }

        feedItem.Message = request.Messsage!;
        feedItem.Edited = request.Edited ?? true;
        feedItem.UpdatedAtDT = now;

        await _appDb.SaveChangesAsync(cancellationToken);

        await _sseConnectionManager.BroadcastCourseAsync(courseId, "update_post", JsonSerializer.Serialize(new UpdatePost(feedItem.Uuid, feedItem.Message, feedItem.Edited), feedSerializerOptions));

        return TypedResults.Ok(FeedItem.FromFeedItem(feedItem));
    }


    [HttpDelete("{postId}")]
    public async Task<Results<NoContent, NotFound>> DeleteFeedItem(Guid courseId, Guid postId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var now = DateTimeOffset.UtcNow;

        var feedItem = await _appDb.FeedItems
            .FirstOrDefaultAsync(feedItem => feedItem.Uuid == postId && feedItem.CourseId == courseId, cancellationToken);

        if (feedItem is null)
        {
            return TypedResults.NotFound();
        }

        _appDb.Remove(feedItem);
        await _appDb.SaveChangesAsync(cancellationToken);

        await _sseConnectionManager.BroadcastCourseAsync(courseId, "delete_post", JsonSerializer.Serialize(new DeletePost(feedItem.Uuid), feedSerializerOptions));
        
        return TypedResults.NoContent();
    }

    [HttpGet("stream")]
    public async Task Stream(Guid courseId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Content-Type", "text/event-stream");

        var connectionId = _sseConnectionManager.AddCourseConnection(courseId, Response, cancellationToken);

        try
        {
            // Keep the connection alive
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken); // heartbeat
            }
        }
        catch (TaskCanceledException)
        {
            // expected on disconnect
        }
        finally
        {
            _sseConnectionManager.RemoveCourseConnection(courseId, connectionId); // clean up
        }
    }
}