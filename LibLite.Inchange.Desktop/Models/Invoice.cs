using System;

namespace LibLite.Inchange.Desktop.Models
{
    class Invoice
    {
        public string Number { get; init; } = string.Empty;
        public DateOnly Issued { get; init; }
    }
}
