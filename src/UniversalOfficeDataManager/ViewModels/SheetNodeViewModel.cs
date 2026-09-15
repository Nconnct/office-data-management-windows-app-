using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.ViewModels
{
    /// <summary>Sidebar tree leaf wrapping a Sheet.</summary>
    public class SheetNodeViewModel
    {
        public Sheet Sheet { get; }
        public int Id => Sheet.Id;
        public int WorkspaceId => Sheet.WorkspaceId;
        public string Name => Sheet.Name;

        public SheetNodeViewModel(Sheet sheet)
        {
            Sheet = sheet;
        }
    }
}
