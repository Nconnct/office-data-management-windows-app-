using System.Windows;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Views
{
    public partial class SheetEditWindow : Window
    {
        public Sheet Sheet { get; }

        public SheetEditWindow(Sheet sheet)
        {
            InitializeComponent();
            Sheet = sheet;
            Title = sheet.Id == 0 ? "New Sheet" : "Edit Sheet";

            NameBox.Text = sheet.Name;
            DescriptionBox.Text = sheet.Description ?? string.Empty;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Sheet name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Sheet.Name = NameBox.Text.Trim();
            Sheet.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
