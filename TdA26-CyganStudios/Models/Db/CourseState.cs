namespace TdA26_CyganStudios.Models.Db;

public enum CourseState
{
    Draft,
    Published,
    Archived,
    Paused,
}

public static class CourseStateExtensions
{
    extension(CourseState state)
    {
        public bool IsPublic => state is CourseState.Published or CourseState.Archived or CourseState.Paused;

        public bool IsStudentEditable => state is CourseState.Published;

        public bool IsLecturerEditable => state is CourseState.Draft or CourseState.Published or CourseState.Paused;
    }
}