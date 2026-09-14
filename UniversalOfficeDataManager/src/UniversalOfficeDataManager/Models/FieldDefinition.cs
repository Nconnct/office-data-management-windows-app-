using System;
using System.Collections.Generic;

namespace UniversalOfficeDataManager.Models
{
    /// <summary>
    /// A user-defined column on a sheet. Stage 1 fields are simple
    /// (name, type, required, default, dropdown options); later stages
    /// add formulas, lookups and linked records on top of this shape.
    /// </summary>
    public class FieldDefinition
    {
        public int Id { get; set; }
        public int SheetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public FieldType FieldType { get; set; } = FieldType.Text;
        public int OrderIndex { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }

        /// <summary>Choices for a Dropdown field. Ignored for other types.</summary>
        public List<string> Options { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
