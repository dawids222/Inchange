using BitMiracle.Docotic.Pdf;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace LibLite.Inchange.Desktop.Services
{
    public interface IFileReader
    {
        Task<string> ReadAsync(string file);
    }

    public class TesseractFileReader : IFileReader
    {
        private const string TEMP_DIRECTORY_NAME = "Temp";

        public Task<string> ReadAsync(string file)
        {
            var type = GetFileType(file);
            var result = type switch
            {
                FileType.PDF => ProcessPdf(file),
                FileType.IMAGE => ProcessImage(file),
                _ => throw new ArgumentOutOfRangeException(nameof(file)),
            };
            return Task.FromResult(result);
        }

        private static FileType GetFileType(string file)
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => FileType.PDF,
                ".jpg" or ".jpeg" or ".png" => FileType.IMAGE,
                _ => throw new NotImplementedException($"nieobsługiwny format '{extension}'")
            };
        }

        private static string ProcessPdf(string file)
        {
            var result = new StringBuilder();

            var dir = Path.GetDirectoryName(file);
            Directory.CreateDirectory($"{dir}\\{TEMP_DIRECTORY_NAME}");

            using var pdf = new PdfDocument(file);
            for (var i = 0; i < pdf.Pages.Count; i++)
            {
                var page = pdf.Pages[i]!;

                // If is searchable, just get the text and move on
                var searchableText = page.GetText();
                if (!string.IsNullOrEmpty(searchableText.Trim()))
                {
                    result.Append(searchableText);
                    continue;
                }

                // Save as an image and process it
                var options = PdfDrawOptions.Create();
                options.BackgroundColor = new PdfRgbColor(255, 255, 255);
                options.HorizontalResolution = 300;
                options.VerticalResolution = 300;

                var tempFile = CreateTempFilePath(file, i);
                page!.Save(tempFile, options);

                try
                {
                    var text = ProcessImage(tempFile);
                    result.Append(text);
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }

            return result.ToString();
        }

        private static string ProcessImage(string file)
        {
            using var engine = new TesseractEngine(@"./tessdata", "pol", EngineMode.Default);
            using var img = Pix.LoadFromFile(file);
            using var page = engine.Process(img);
            return page.GetText();
        }

        private static string CreateTempFilePath(string file, int i)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var dir = Path.GetDirectoryName(file)!;
            var ext = Path.GetExtension(file);
            return Path.Combine(dir, $"{TEMP_DIRECTORY_NAME}\\{name}_temp_{i + 1}" + ext);
        }

        private enum FileType
        {
            PDF,
            IMAGE,
        }
    }
}
