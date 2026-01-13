using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record MaterialsGridModel(Guid CourseUuid, ICollection<DbMaterial> Materials, bool Editable);