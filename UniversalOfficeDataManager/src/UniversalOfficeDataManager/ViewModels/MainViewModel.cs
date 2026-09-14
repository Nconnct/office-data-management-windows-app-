using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalOfficeDataManager.Models;
using UniversalOfficeDataManager.Repositories;
using UniversalOfficeDataManager.Services;
using UniversalOfficeDataManager.ViewModels.Common;

namespace UniversalOfficeDataManager.ViewModels
{
    /// <summary>
    /// Root view-model: owns the sidebar (workspaces/sheets), navigation
    /// between Dashboard and an open sheet, and every workspace/sheet
    /// CRUD command reachable from the sidebar.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ISheetRepository _sheetRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IRecordRepository _recordRepository;
        private readonly IDialogService _dialogService;

        public ObservableCollection<WorkspaceNodeViewModel> Workspaces { get; } = new();
        public DashboardViewModel Dashboard { get; }

        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private SheetNodeViewModel? _selectedSheetNode;
        public SheetNodeViewModel? SelectedSheetNode
        {
            get => _selectedSheetNode;
            set
            {
                if (SetProperty(ref _selectedSheetNode, value) && value != null)
                    _ = OpenSheetAsync(value.Sheet);
            }
        }

        public ICommand ShowDashboardCommand { get; }
        public ICommand AddWorkspaceCommand { get; }
        public ICommand EditWorkspaceCommand { get; }
        public ICommand DeleteWorkspaceCommand { get; }
        public ICommand AddSheetCommand { get; }
        public ICommand EditSheetCommand { get; }
        public ICommand DeleteSheetCommand { get; }

        public MainViewModel(
            IWorkspaceRepository workspaceRepository,
            ISheetRepository sheetRepository,
            IFieldRepository fieldRepository,
            IRecordRepository recordRepository,
            IDialogService dialogService)
        {
            _workspaceRepository = workspaceRepository;
            _sheetRepository = sheetRepository;
            _fieldRepository = fieldRepository;
            _recordRepository = recordRepository;
            _dialogService = dialogService;

            Dashboard = new DashboardViewModel(_workspaceRepository, _sheetRepository, _recordRepository);
            CurrentView = Dashboard;

            ShowDashboardCommand = new AsyncRelayCommand(ShowDashboardAsync);
            AddWorkspaceCommand = new AsyncRelayCommand(AddWorkspaceAsync);
            EditWorkspaceCommand = new AsyncRelayCommand(
                p => EditWorkspaceAsync(p as WorkspaceNodeViewModel), p => p is WorkspaceNodeViewModel);
            DeleteWorkspaceCommand = new AsyncRelayCommand(
                p => DeleteWorkspaceAsync(p as WorkspaceNodeViewModel), p => p is WorkspaceNodeViewModel);
            AddSheetCommand = new AsyncRelayCommand(
                p => AddSheetAsync(p as WorkspaceNodeViewModel), p => p is WorkspaceNodeViewModel);
            EditSheetCommand = new AsyncRelayCommand(
                p => EditSheetAsync(p as SheetNodeViewModel), p => p is SheetNodeViewModel);
            DeleteSheetCommand = new AsyncRelayCommand(
                p => DeleteSheetAsync(p as SheetNodeViewModel), p => p is SheetNodeViewModel);

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            Workspaces.Clear();

            foreach (var workspace in await _workspaceRepository.GetAllAsync())
            {
                var node = new WorkspaceNodeViewModel(workspace);
                foreach (var sheet in await _sheetRepository.GetByWorkspaceAsync(workspace.Id))
                    node.Sheets.Add(new SheetNodeViewModel(sheet));
                Workspaces.Add(node);
            }

            await Dashboard.RefreshAsync();
        }

        private async Task ShowDashboardAsync()
        {
            await Dashboard.RefreshAsync();
            CurrentView = Dashboard;
        }

        private async Task AddWorkspaceAsync(object? parameter)
        {
            var workspace = new Workspace();
            if (_dialogService.ShowWorkspaceEditor(workspace) == true)
            {
                await _workspaceRepository.CreateAsync(workspace);
                await LoadAsync();
            }
        }

        private async Task EditWorkspaceAsync(WorkspaceNodeViewModel? node)
        {
            if (node == null) return;

            if (_dialogService.ShowWorkspaceEditor(node.Workspace) == true)
            {
                await _workspaceRepository.UpdateAsync(node.Workspace);
                await LoadAsync();
            }
        }

        private async Task DeleteWorkspaceAsync(WorkspaceNodeViewModel? node)
        {
            if (node == null) return;

            if (!_dialogService.ConfirmDelete(
                    $"Delete workspace \"{node.Name}\" and all its sheets and records? This cannot be undone."))
                return;

            var wasOpen = CurrentView is SheetDetailViewModel detail && detail.Sheet.WorkspaceId == node.Id;

            await _workspaceRepository.DeleteAsync(node.Id);
            await LoadAsync();

            if (wasOpen)
                await ShowDashboardAsync();
        }

        private async Task AddSheetAsync(WorkspaceNodeViewModel? node)
        {
            if (node == null) return;

            var sheet = new Sheet { WorkspaceId = node.Id };
            if (_dialogService.ShowSheetEditor(sheet) == true)
            {
                await _sheetRepository.CreateAsync(sheet);
                await LoadAsync();
            }
        }

        private async Task EditSheetAsync(SheetNodeViewModel? node)
        {
            if (node == null) return;

            if (_dialogService.ShowSheetEditor(node.Sheet) == true)
            {
                await _sheetRepository.UpdateAsync(node.Sheet);
                await LoadAsync();
            }
        }

        private async Task DeleteSheetAsync(SheetNodeViewModel? node)
        {
            if (node == null) return;

            if (!_dialogService.ConfirmDelete(
                    $"Delete sheet \"{node.Name}\" and all its records? This cannot be undone."))
                return;

            var wasOpen = CurrentView is SheetDetailViewModel detail && detail.Sheet.Id == node.Id;

            await _sheetRepository.DeleteAsync(node.Id);
            await LoadAsync();

            if (wasOpen)
                await ShowDashboardAsync();
        }

        private async Task OpenSheetAsync(Sheet sheet)
        {
            var detail = new SheetDetailViewModel(sheet, _fieldRepository, _recordRepository, _dialogService);
            await detail.LoadAsync();
            CurrentView = detail;
        }
    }
}
