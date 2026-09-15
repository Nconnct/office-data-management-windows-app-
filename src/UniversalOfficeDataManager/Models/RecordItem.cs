using System;
using System.Collections.Generic;

namespace UniversalOfficeDataManager.Models
{
    /// <summary>
    /// One row in a sheet. Values are stored as a FieldId -> raw string
    /// map (an EAV-style layout) rather than fixed columns, since every
    /// sheet's fields are user-defined and can change at any time.
    /// </summary>
    public class RecordItem
    {
        public int Id { get; set; }
        public int SheetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public Dictionary<int, string?> Values { get; set; } = new();
    }
}
