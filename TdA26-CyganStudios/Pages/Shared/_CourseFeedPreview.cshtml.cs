using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record CourseFeedPreviewModel(Guid CourseId, IEnumerable<DbFeedItem> Items, int MaxPreviewCount = 4);