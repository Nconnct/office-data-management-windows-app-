using System.Collections.ObjectModel;
using UniversalOfficeDataManager.Models;
using UniversalOfficeDataManager.ViewModels.Common;

namespace UniversalOfficeDataManager.ViewModels
{
    /// <summary>Sidebar tree node wrapping a Workspace and its Sheets.</summary>
    public class WorkspaceNodeViewModel : ObservableObject
    {
        public Workspace Workspace { get; }
        public ObservableCollection<SheetNodeViewModel> Sheets { get; } = new();

        public int Id => Workspace.Id;
        public string Name => Workspace.Name;

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public WorkspaceNodeViewModel(Workspace workspace)
        {
            Workspace = workspace;
        }
    }
}
