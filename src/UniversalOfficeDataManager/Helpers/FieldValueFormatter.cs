using System;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Helpers
{
    /// <summary>
    /// Turns a raw stored value into the text shown in the read-only
    /// records grid. Parsing/formatting failures fall back to the raw
    /// string rather than throwing, since a record saved under an older
    /// field type should still display something reasonable.
    /// </summary>
    public static class FieldValueFormatter
    {
        public static string Format(FieldDefinition field, string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            switch (field.FieldType)
            {
                case FieldType.Currency:
                    return decimal.TryParse(raw, out var currencyValue)
                        ? currencyValue.ToString("N2")
                        : raw;

                case FieldType.Percentage:
                    return decimal.TryParse(raw, out var percentValue)
                        ? percentValue.ToString("0.##") + "%"
                        : raw;

                case FieldType.Decimal:
                    return decimal.TryParse(raw, out var decimalValue)
                        ? decimalValue.ToString("0.##")
                        : raw;

                case FieldType.Number:
                    return long.TryParse(raw, out var numberValue)
                        ? numberValue.ToString("N0")
                        : raw;

                case FieldType.YesNo:
                    return raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase)
                        ? "Yes"
                        : "No";

                case FieldType.Date:
                    return DateTime.TryParse(raw, out var dateValue)
                        ? dateValue.ToString("dd MMM yyyy")
                        : raw;

                default:
                    return raw;
            }
        }
    }
}
