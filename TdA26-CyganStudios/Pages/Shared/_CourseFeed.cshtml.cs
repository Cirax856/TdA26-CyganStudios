using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record CourseFeedModel(Guid CourseId, IEnumerable<DbFeedItem> Items, bool Editable);