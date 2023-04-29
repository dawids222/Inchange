using IronOcr;
using System.Threading.Tasks;

namespace LibLite.Inchange.Desktop.Services
{
    public interface IFileReader
    {
        Task<string> ReadAsync(string file);
    }

    public class TesseractFileReader : IFileReader
    {
        private readonly IronTesseract _reader = new()
        {
            Language = OcrLanguage.Polish,
        };

        public async Task<string> ReadAsync(string file)
        {
            var result = await _reader.ReadAsync(file);
            return result.Text;
        }
    }
}
