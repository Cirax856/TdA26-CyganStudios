using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record MaterialsPreviewGridModel(Guid CourseUuid, ICollection<DbMaterial> Materials, bool Editable);