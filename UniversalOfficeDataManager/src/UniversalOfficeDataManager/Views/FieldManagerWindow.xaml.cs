using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using UniversalOfficeDataManager.Models;
using UniversalOfficeDataManager.Repositories;

namespace UniversalOfficeDataManager.Views
{
    public partial class FieldManagerWindow : Window
    {
        private readonly Sheet _sheet;
        private readonly IFieldRepository _fieldRepository;
        public ObservableCollection<FieldDefinition> Fields { get; } = new();

        public FieldManagerWindow(Sheet sheet, IFieldRepository fieldRepository)
        {
            InitializeComponent();
            _sheet = sheet;
            _fieldRepository = fieldRepository;
            Title = $"Manage Fields — {sheet.Name}";

            FieldsList.ItemsSource = Fields;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            Fields.Clear();
            foreach (var field in await _fieldRepository.GetBySheetAsync(_sheet.Id))
                Fields.Add(field);
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var newField = new FieldDefinition { SheetId = _sheet.Id, OrderIndex = Fields.Count };
            var dialog = new FieldEditWindow(newField) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                await _fieldRepository.CreateAsync(newField);
                await LoadAsync();
            }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsList.SelectedItem is not FieldDefinition selected)
            {
                MessageBox.Show("Select a field first.", "Manage Fields", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new FieldEditWindow(selected) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                await _fieldRepository.UpdateAsync(selected);
                await LoadAsync();
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsList.SelectedItem is not FieldDefinition selected)
            {
                MessageBox.Show("Select a field first.", "Manage Fields", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Delete field \"{selected.Name}\"? Any values already stored in this field will also be deleted.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _fieldRepository.DeleteAsync(selected.Id);
                await LoadAsync();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
