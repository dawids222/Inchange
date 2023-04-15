using Humanizer;
using IronOcr;
using LibLite.Inchange.Desktop.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace LibLite.Inchange.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            var progress = new ProgressWindow();

            if (dialog.ShowDialog() == true)
            {
                // TODO: Make it do anything... Now it freeze.
                progress.Show();

                var files = dialog.FileNames;
                // TODO: Add files processing here.

                foreach (var file in files)
                {
                    // TODO: Extract?
                    var content = _reader.Read(file).Text;
                    // Check if even is an invoce (has all the keywords)
                    var isInvoice = IsInvoice(content);
                    if (!isInvoice) { continue; }


                    var directory = Path.GetDirectoryName(file);
                    var info = Directory.CreateDirectory($"{directory}\\Result");

                    var name = CreateFileName(content);
                    var extension = Path.GetExtension(file);
                    var path = $"{info.FullName}\\{name}{extension}";

                    File.Copy(file, path, true);
                }
            }

            progress.Hide();

            MessageBox.Show("Done! :)");

            Environment.Exit(0);
        }

        // TODO: Rename.
        private string CreateFileName(string content)
        {
            // Get issuer, number, issue date
            var invoice = GetInvoiceData(content);
            // Form a filename
            var issued = invoice.Issued.ToString("yyyy.MM.dd");
            var number = invoice.Number.Replace('/', '-');
            var issuer = invoice.Issuer.Pascalize();
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
            public string Issuer { get; set; } = string.Empty;
        }
        private static Invoice GetInvoiceData(string content)
        {
            var number = GetNumber(content);
            var issued = GetFirstDate(content);
            var issuer = "";

            return new Invoice
            {
                Number = number,
                Issued = issued,
                Issuer = issuer,
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
            var match = regexes
                .Select(x => x.Match(content))
                .Where(x => x.Success)
                .First();
            var datetime = DateTime.ParseExact(match.Value, _datetimeFormats, CultureInfo.InvariantCulture);
            return DateOnly.FromDateTime(datetime);
        }
    }
}
