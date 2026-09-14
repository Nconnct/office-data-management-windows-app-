using System;

namespace UniversalOfficeDataManager.Models
{
    /// <summary>
    /// A top-level container for sheets — e.g. "Shop Inventory",
    /// "HR", "Client Accounts".
    /// </summary>
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorHex { get; set; } = "#2563EB";
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
