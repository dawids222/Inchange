using IronOcr;
using LibLite.Inchange.Desktop.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LibLite.Inchange.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ProgressWindow _progress = new();
        private readonly IronTesseract _reader = new()
        {
            Language = OcrLanguage.Polish,
        };

        public MainWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;

            var dialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Invoices|*.pdf;*.jpg;*.jpeg;*.png",
            };

            if (dialog.ShowDialog() == true)
            {
                // TODO: Make it do anything... Now it freeze.
                _progress.Show();

                var files = dialog.FileNames;
                // TODO: Add files processing here.

                var context = SynchronizationContext.Current;
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    await Parallel.ForEachAsync(files,
                        new ParallelOptions { MaxDegreeOfParallelism = 4 },
                        (file, token) =>
                        {
                            // TODO: Extract?
                            var content = _reader.Read(file).Text;
                            // Check if even is an invoce (has all the keywords)
                            // What if is not an invoive

                            //var isInvoice = IsInvoice(content);
                            //if (!isInvoice) { continue; }

                            var directory = Path.GetDirectoryName(file);
                            var info = Directory.CreateDirectory($"{directory}\\Result");

                            var name = CreateFileName(content);
                            // TODO: What if there is any kind of error?

                            var extension = Path.GetExtension(file);
                            var path = $"{info.FullName}\\{name}{extension}";

                            File.Copy(file, path, true);

                            context!.Post(_ =>
                            {
                                _progress.AddProgress(100 / files.Length);
                            }, null);

                            return ValueTask.CompletedTask;
                        });

                    context!.Post(_ =>
                    {
                        _progress.Hide();
                        MessageBox.Show("Done! :)");
                        Environment.Exit(0);
                    }, null);
                });
            }
        }

        // TODO: Rename.
        private static string CreateFileName(string content)
        {
            var invoice = GetInvoiceData(content);
            var issued = invoice.Issued.ToString("yyyy.MM.dd");
            var number = invoice.Number.Replace('/', '-');
            return $"{issued}_{number}";
        }

        private readonly Dictionary<string, string[]> _invoiceKeywords = new()
        {
            { "name", new string[] { "faktura" } },
            { "issued", new string[] { "data wystawienia", "wystawiono dnia" } },
            { "issuer", new string[] { "sprzedawca" } },
        };
        private bool IsInvoice(string content)
        {
            return _invoiceKeywords
                .All(entry => entry.Value
                    .Any(keyword => content.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        class Invoice
        {
            public string Number { get; set; } = string.Empty;
            public DateOnly Issued { get; set; }
        }
        private static Invoice GetInvoiceData(string content)
        {
            var number = GetNumber(content);
            var issued = GetFirstDate(content);

            return new Invoice
            {
                Number = number,
                Issued = issued,
            };
        }

        private static string GetNumber(string content)
        {
            var regex = new Regex(@"\b\S*[\/]\S*\b");
            var matches = regex.Matches(content);
            return matches
                .Select(x => x.Value)
                .OrderByDescending(x => x.Split('/').Length)
                .First();
        }

        private static readonly string[] _datetimeFormats = {
            "dd/MM/yyyy",
            "yyyy/MM/dd",
            "dd-MM-yyyy",
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "yyyy.MM.dd",
        };
        private static DateOnly GetFirstDate(string content)
        {
            var regexes = new Regex[]
            {
                new Regex(@"\b\d{2}[\.\/-]\d{2}[\.\/-]\d{4}\b"),
                new Regex(@"\b\d{4}[\.\/-]\d{2}[\.\/-]\d{2}\b"),
            };
            var matches = regexes
                .Select(x => x.Matches(content))
                .SelectMany(x => x.ToList())
                .ToList();
            var datetime = matches
                .Where(x => x.Success)
                .Select(x =>
                {
                    DateTime.TryParseExact(x.Value, _datetimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime);
                    return datetime;
                })
                .First(x => x != default);
            return DateOnly.FromDateTime(datetime);
        }
    }
}
