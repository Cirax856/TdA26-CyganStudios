using System.Text.Json.Serialization;

namespace TdA26_CyganStudios.Models.Api;

public sealed record FeedItem(Guid Uuid, FeedItemType Type, string Message, bool? Edited, DateTime CreatedAt, DateTime? UpdatedAt);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FeedItemType
{
    [JsonStringEnumMemberName("manual")] Manual,
    [JsonStringEnumMemberName("system")] System,
}
