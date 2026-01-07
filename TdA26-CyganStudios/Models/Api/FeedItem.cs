using System.Text.Json.Serialization;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Models.Api;

public sealed record FeedItem(Guid Uuid, FeedItemType Type, string Message, bool? Edited, DateTime CreatedAt, DateTime? UpdatedAt)
{
    public static FeedItem FromFeedItem(DbFeedItem feedItem)
        => new FeedItem(feedItem.Uuid, feedItem.Type switch { DbFeedItemType.Manual => FeedItemType.Manual, DbFeedItemType.System => FeedItemType.System, _ => default }, feedItem.Message, feedItem.Edited, feedItem.CreatedAtDT.DateTime, feedItem.UpdatedAtDT?.DateTime);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FeedItemType
{
    [JsonStringEnumMemberName("manual")] Manual,
    [JsonStringEnumMemberName("system")] System,
}
