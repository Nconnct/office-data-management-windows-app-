using System;

namespace UniversalOfficeDataManager.Models
{
    /// <summary>
    /// A single tracker inside a workspace — e.g. "Attendance",
    /// "Payments", "Customers". Holds a set of fields and records.
    /// </summary>
    public class Sheet
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
