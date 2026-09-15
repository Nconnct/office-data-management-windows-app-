using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalOfficeDataManager.Models;
using UniversalOfficeDataManager.Repositories;
using UniversalOfficeDataManager.Services;
using UniversalOfficeDataManager.ViewModels.Common;

namespace UniversalOfficeDataManager.ViewModels
{
    /// <summary>
    /// Backs the read-only records grid for one open sheet, plus the
    /// "Manage Fields" / add / edit / delete record actions.
    /// </summary>
    public class SheetDetailViewModel : ObservableObject
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IRecordRepository _recordRepository;
        private readonly IDialogService _dialogService;

        public Sheet Sheet { get; }
        public ObservableCollection<FieldDefinition> Fields { get; } = new();
        public ObservableCollection<RecordRowViewModel> Rows { get; } = new();

        private RecordRowViewModel? _selectedRow;
        public RecordRowViewModel? SelectedRow
        {
            get => _selectedRow;
            set => SetProperty(ref _selectedRow, value);
        }

        public ICommand ManageFieldsCommand { get; }
        public ICommand AddRecordCommand { get; }
        public ICommand EditRecordCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand RefreshCommand { get; }

        public SheetDetailViewModel(
            Sheet sheet,
            IFieldRepository fieldRepository,
            IRecordRepository recordRepository,
            IDialogService dialogService)
        {
            Sheet = sheet;
            _fieldRepository = fieldRepository;
            _recordRepository = recordRepository;
            _dialogService = dialogService;

            ManageFieldsCommand = new AsyncRelayCommand(ManageFieldsAsync);
            AddRecordCommand = new AsyncRelayCommand(AddRecordAsync);
            EditRecordCommand = new AsyncRelayCommand(EditRecordAsync, () => SelectedRow != null);
            DeleteRecordCommand = new AsyncRelayCommand(DeleteRecordAsync, () => SelectedRow != null);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
        }

        public async Task LoadAsync()
        {
            Fields.Clear();
            foreach (var field in await _fieldRepository.GetBySheetAsync(Sheet.Id))
                Fields.Add(field);

            var fieldSnapshot = Fields.ToList();

            Rows.Clear();
            foreach (var record in await _recordRepository.GetBySheetAsync(Sheet.Id))
                Rows.Add(new RecordRowViewModel(record, fieldSnapshot));
        }

        private async Task ManageFieldsAsync()
        {
            _dialogService.ShowFieldManager(Sheet, _fieldRepository);
            await LoadAsync();
        }

        private async Task AddRecordAsync()
        {
            if (Fields.Count == 0)
            {
                _dialogService.ShowError(
                    "Add at least one field to this sheet before creating records. Use \"Manage Fields\" first.");
                return;
            }

            var newRecord = new RecordItem { SheetId = Sheet.Id };
            if (_dialogService.ShowRecordEditor(Fields.ToList(), newRecord) == true)
            {
                await _recordRepository.CreateAsync(newRecord);
                await LoadAsync();
            }
        }

        private async Task EditRecordAsync()
        {
            if (SelectedRow == null) return;

            if (_dialogService.ShowRecordEditor(Fields.ToList(), SelectedRow.Record) == true)
            {
                await _recordRepository.UpdateAsync(SelectedRow.Record);
                await LoadAsync();
            }
        }

        private async Task DeleteRecordAsync()
        {
            if (SelectedRow == null) return;

            if (_dialogService.ConfirmDelete("Delete this record? This cannot be undone."))
            {
                await _recordRepository.DeleteAsync(SelectedRow.Record.Id);
                await LoadAsync();
            }
        }
    }
}
