namespace TdA26_CyganStudios.Utils;

public static class FileSize
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public static string ToString(long sizeInBytes)
    {
        double size = sizeInBytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < Units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {Units[unitIndex]}";
    }
}