using System.Windows;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Views
{
    public partial class WorkspaceEditWindow : Window
    {
        public Workspace Workspace { get; }

        public WorkspaceEditWindow(Workspace workspace)
        {
            InitializeComponent();
            Workspace = workspace;
            Title = workspace.Id == 0 ? "New Workspace" : "Edit Workspace";

            NameBox.Text = workspace.Name;
            DescriptionBox.Text = workspace.Description ?? string.Empty;
            ColorBox.Text = workspace.ColorHex;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Workspace name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Workspace.Name = NameBox.Text.Trim();
            Workspace.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
            Workspace.ColorHex = string.IsNullOrWhiteSpace(ColorBox.Text) ? "#2563EB" : ColorBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
