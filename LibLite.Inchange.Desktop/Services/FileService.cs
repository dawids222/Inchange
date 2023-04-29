using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Document = LibLite.Inchange.Desktop.Models.Document;

namespace LibLite.Inchange.Desktop.Services
{
    public interface IFileService
    {
        Task<Document> GetDocumentAsync(string file);
        void Copy(Document document, string name);
    }

    public class FileService : IFileService
    {
        private static readonly Regex _sanitizationRegex = new("/{2,}");

        private readonly IFileReader _reader;

        public FileService(IFileReader reader)
        {
            _reader = reader;
        }

        public async Task<Document> GetDocumentAsync(string file)
        {
            var content = await _reader.ReadAsync(file);
            content = Sanitize(content);
            return new Document
            {
                Content = content,
                Path = file,
            };
        }

        private static string Sanitize(string content)
        {
            return _sanitizationRegex.Replace(content, "/");
        }

        public void Copy(Document document, string name)
        {
            var directory = Path.GetDirectoryName(document.Path);
            var info = Directory.CreateDirectory($"{directory}\\Result");
            var extension = Path.GetExtension(document.Path);
            var destination = $"{info.FullName}\\{name}{extension}";
            File.Copy(document.Path, destination, true);
        }
    }
}
