namespace LibLite.Inchange.Desktop.Models
{
    public class Document
    {
        public string Path { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;

        public string Name => System.IO.Path.GetFileName(Path);
    }
}
