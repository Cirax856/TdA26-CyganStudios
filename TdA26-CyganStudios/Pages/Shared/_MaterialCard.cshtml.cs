using TdA26_CyganStudios;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed record MaterialCardModel(DbMaterial Material, Guid CourseUuid, bool Editable);