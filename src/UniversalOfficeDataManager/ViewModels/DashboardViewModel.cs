using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UniversalOfficeDataManager.Repositories;
using UniversalOfficeDataManager.ViewModels.Common;

namespace UniversalOfficeDataManager.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ISheetRepository _sheetRepository;
        private readonly IRecordRepository _recordRepository;

        private int _totalWorkspaces;
        public int TotalWorkspaces
        {
            get => _totalWorkspaces;
            set => SetProperty(ref _totalWorkspaces, value);
        }

        private int _totalSheets;
        public int TotalSheets
        {
            get => _totalSheets;
            set => SetProperty(ref _totalSheets, value);
        }

        private int _totalRecords;
        public int TotalRecords
        {
            get => _totalRecords;
            set => SetProperty(ref _totalRecords, value);
        }

        public ObservableCollection<SheetSummary> RecentSheets { get; } = new();

        public DashboardViewModel(
            IWorkspaceRepository workspaceRepository,
            ISheetRepository sheetRepository,
            IRecordRepository recordRepository)
        {
            _workspaceRepository = workspaceRepository;
            _sheetRepository = sheetRepository;
            _recordRepository = recordRepository;
        }

        public async Task RefreshAsync()
        {
            TotalWorkspaces = await _workspaceRepository.CountAsync();
            TotalSheets = await _sheetRepository.CountAllAsync();
            TotalRecords = await _recordRepository.CountAllAsync();

            var workspaces = await _workspaceRepository.GetAllAsync();
            var summaries = new List<SheetSummary>();

            foreach (var workspace in workspaces)
            {
                foreach (var sheet in await _sheetRepository.GetByWorkspaceAsync(workspace.Id))
                {
                    summaries.Add(new SheetSummary
                    {
                        WorkspaceName = workspace.Name,
                        SheetName = sheet.Name,
                        RecordCount = await _recordRepository.CountBySheetAsync(sheet.Id),
                        ModifiedAt = sheet.ModifiedAt
                    });
                }
            }

            RecentSheets.Clear();
            foreach (var summary in summaries.OrderByDescending(s => s.ModifiedAt).Take(8))
                RecentSheets.Add(summary);
        }
    }

    public class SheetSummary
    {
        public string WorkspaceName { get; set; } = string.Empty;
        public string SheetName { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
