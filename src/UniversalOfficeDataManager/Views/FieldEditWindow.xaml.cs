using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Views
{
    public partial class FieldEditWindow : Window
    {
        public FieldDefinition Field { get; }

        public FieldEditWindow(FieldDefinition field)
        {
            InitializeComponent();
            Field = field;
            Title = field.Id == 0 ? "Add Field" : "Edit Field";

            TypeCombo.ItemsSource = Enum.GetValues(typeof(FieldType));
            TypeCombo.SelectedItem = field.FieldType;

            NameBox.Text = field.Name;
            RequiredCheck.IsChecked = field.IsRequired;
            DefaultValueBox.Text = field.DefaultValue ?? string.Empty;
            OptionsBox.Text = string.Join(", ", field.Options);

            OptionsPanel.Visibility = field.FieldType == FieldType.Dropdown ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var isDropdown = TypeCombo.SelectedItem is FieldType selected && selected == FieldType.Dropdown;
            OptionsPanel.Visibility = isDropdown ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Field name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedType = TypeCombo.SelectedItem is FieldType type ? type : FieldType.Text;

            if (selectedType == FieldType.Dropdown &&
                OptionsBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length == 0)
            {
                MessageBox.Show("Add at least one option for a Dropdown field.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Field.Name = NameBox.Text.Trim();
            Field.FieldType = selectedType;
            Field.IsRequired = RequiredCheck.IsChecked == true;
            Field.DefaultValue = string.IsNullOrWhiteSpace(DefaultValueBox.Text) ? null : DefaultValueBox.Text.Trim();
            Field.Options = OptionsBox.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
