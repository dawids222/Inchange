using LibLite.Inchange.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Document = LibLite.Inchange.Desktop.Models.Document;

namespace LibLite.Inchange.Desktop.Services
{
    public interface IInvoiceService
    {
        string GenerateName(Document document);
    }

    public class InvoiceService : IInvoiceService
    {
        private static readonly Regex _numberRegex = new(@"\b\S*[\/]\S*\b");

        private static readonly IEnumerable<Regex> _dateRegexes = new Regex[]
        {
            new(@"\b\d{2}[\.\/-]\d{2}[\.\/-]\d{4}\b"),
            new(@"\b\d{4}[\.\/-]\d{2}[\.\/-]\d{2}\b"),
        };

        private static readonly string[] _datetimeFormats = {
            "dd/MM/yyyy",
            "yyyy/MM/dd",
            "dd-MM-yyyy",
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "yyyy.MM.dd",
        };

        public string GenerateName(Document document)
        {
            var invoice = GetInvoice(document);
            var issued = invoice.Issued.ToString("yyyy.MM.dd");
            var number = invoice.Number.Replace('/', '-');
            return $"fv_{issued}_{number}";
        }

        private static Invoice GetInvoice(Document document)
        {
            var number = GetNumber(document);
            var issued = GetFirstDate(document);

            return new Invoice
            {
                Number = number,
                Issued = issued,
            };
        }

        private static string GetNumber(Document document)
        {
            var matches = _numberRegex.Matches(document.Content);
            var result = matches
                .Select(x => x.Value)
                .OrderByDescending(x => x.Split('/').Length)
                .FirstOrDefault();

            if (result == default)
            {
                throw new Exception("Nie można znaleźć numeru faktury");
            }

            return result;
        }

        private static DateOnly GetFirstDate(Document document)
        {
            var matches = _dateRegexes
                .Select(x => x.Matches(document.Content))
                .SelectMany(x => x.ToList())
                .ToList();

            var result = matches
                .Where(x => x.Success)
                .Select(x =>
                {
                    DateTime.TryParseExact(x.Value, _datetimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime);
                    return datetime;
                })
                .FirstOrDefault(x => x != default);

            if (result == default)
            {
                throw new Exception("Nie można znaleźć daty wystawienia");
            }

            return DateOnly.FromDateTime(result);
        }
    }
}
