using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Views
{
    /// <summary>
    /// Builds a data-entry form from a sheet's field definitions at
    /// runtime. Built in code-behind (rather than a XAML
    /// DataTemplateSelector) since the set and order of fields is only
    /// known once this window is constructed.
    /// </summary>
    public partial class RecordEditWindow : Window
    {
        private readonly List<FieldDefinition> _fields;
        private readonly RecordItem _record;
        private readonly Dictionary<int, FrameworkElement> _editors = new();

        public RecordEditWindow(List<FieldDefinition> fields, RecordItem record)
        {
            InitializeComponent();
            _fields = fields.OrderBy(f => f.OrderIndex).ThenBy(f => f.Id).ToList();
            _record = record;
            Title = record.Id == 0 ? "Add Record" : "Edit Record";
            BuildForm();
        }

        private void BuildForm()
        {
            FieldsPanel.Children.Clear();
            _editors.Clear();

            foreach (var field in _fields)
            {
                var label = new TextBlock
                {
                    Text = field.IsRequired ? field.Name + " *" : field.Name,
                    Margin = new Thickness(0, 10, 0, 4),
                    FontWeight = FontWeights.SemiBold
                };
                FieldsPanel.Children.Add(label);

                _record.Values.TryGetValue(field.Id, out var rawValue);
                if (string.IsNullOrEmpty(rawValue))
                    rawValue = field.DefaultValue;

                var editor = CreateEditor(field, rawValue);
                FieldsPanel.Children.Add(editor);
                _editors[field.Id] = editor;
            }

            if (_fields.Count == 0)
            {
                FieldsPanel.Children.Add(new TextBlock
                {
                    Text = "This sheet has no fields yet. Add fields first using \"Manage Fields\".",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Gray
                });
            }
        }

        private static FrameworkElement CreateEditor(FieldDefinition field, string? rawValue)
        {
            switch (field.FieldType)
            {
                case FieldType.YesNo:
                    return new CheckBox
                    {
                        IsChecked = rawValue == "1" || string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase)
                    };

                case FieldType.Date:
                    var datePicker = new DatePicker();
                    if (DateTime.TryParse(rawValue, out var dateValue))
                        datePicker.SelectedDate = dateValue;
                    return datePicker;

                case FieldType.Dropdown:
                    var comboBox = new ComboBox();
                    foreach (var option in field.Options)
                        comboBox.Items.Add(option);
                    if (!string.IsNullOrEmpty(rawValue))
                        comboBox.SelectedItem = rawValue;
                    return comboBox;

                case FieldType.Address:
                    return new TextBox
                    {
                        Text = rawValue ?? string.Empty,
                        Padding = new Thickness(6),
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.Wrap,
                        Height = 60
                    };

                default:
                    return new TextBox
                    {
                        Text = rawValue ?? string.Empty,
                        Padding = new Thickness(6)
                    };
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var newValues = new Dictionary<int, string?>();

            foreach (var field in _fields)
            {
                if (!_editors.TryGetValue(field.Id, out var editor))
                    continue;

                string? value = editor switch
                {
                    CheckBox cb => cb.IsChecked == true ? "1" : "0",
                    DatePicker dp => dp.SelectedDate?.ToString("O"),
                    ComboBox combo => combo.SelectedItem as string ?? (string.IsNullOrEmpty(combo.Text) ? null : combo.Text),
                    TextBox tb => tb.Text,
                    _ => null
                };

                if (field.IsRequired && string.IsNullOrWhiteSpace(value) && field.FieldType != FieldType.YesNo)
                {
                    MessageBox.Show($"\"{field.Name}\" is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (field.FieldType == FieldType.Number && !string.IsNullOrWhiteSpace(value) && !long.TryParse(value, out _))
                {
                    MessageBox.Show($"\"{field.Name}\" must be a whole number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if ((field.FieldType == FieldType.Decimal || field.FieldType == FieldType.Currency || field.FieldType == FieldType.Percentage)
                    && !string.IsNullOrWhiteSpace(value) && !decimal.TryParse(value, out _))
                {
                    MessageBox.Show($"\"{field.Name}\" must be a number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (field.FieldType == FieldType.Email && !string.IsNullOrWhiteSpace(value) && !value.Contains('@'))
                {
                    MessageBox.Show($"\"{field.Name}\" must be a valid email address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                newValues[field.Id] = value;
            }

            foreach (var kvp in newValues)
                _record.Values[kvp.Key] = kvp.Value;

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
