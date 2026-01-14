using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios.Pages.Shared;

public sealed record class FeedItemCardModel(DbFeedItem Item);
