using System.Windows.Controls;
using System.Windows.Data;
using UniversalOfficeDataManager.ViewModels;

namespace UniversalOfficeDataManager.Views
{
    /// <summary>
    /// The records DataGrid's columns depend on the sheet's field
    /// definitions, which are only known at runtime, so columns are built
    /// in code-behind rather than declared in XAML. They are rebuilt
    /// whenever the Fields collection changes (e.g. after "Manage Fields").
    /// </summary>
    public partial class SheetDetailView : UserControl
    {
        private SheetDetailViewModel? _attachedViewModel;

        public SheetDetailView()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => AttachViewModel();
        }

        private void AttachViewModel()
        {
            if (_attachedViewModel != null)
                _attachedViewModel.Fields.CollectionChanged -= FieldsChanged;

            _attachedViewModel = DataContext as SheetDetailViewModel;
            if (_attachedViewModel == null) return;

            _attachedViewModel.Fields.CollectionChanged += FieldsChanged;
            RebuildColumns(_attachedViewModel);
        }

        private void FieldsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_attachedViewModel != null)
                RebuildColumns(_attachedViewModel);
        }

        private void RebuildColumns(SheetDetailViewModel viewModel)
        {
            RecordsGrid.Columns.Clear();
            foreach (var field in viewModel.Fields)
            {
                RecordsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = field.Name,
                    Binding = new Binding($"[{field.Id}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    IsReadOnly = true
                });
            }
        }
    }
}
