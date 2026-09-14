namespace UniversalOfficeDataManager.Models
{
    /// <summary>
    /// Field types supported by Stage 1. This will grow in later stages to
    /// include Multi-select, Status, Priority, Checkbox, Image, PDF, File,
    /// Signature, Barcode, QR, Auto-ID, Formula, Calculated, Lookup and
    /// Linked Record — deliberately left out until the features that make
    /// them meaningful (formula engine, attachments, linked sheets) exist.
    /// </summary>
    public enum FieldType
    {
        Text,
        Number,
        Decimal,
        Currency,
        Percentage,
        Phone,
        Email,
        Address,
        Date,
        Time,
        Dropdown,
        YesNo
    }
}
