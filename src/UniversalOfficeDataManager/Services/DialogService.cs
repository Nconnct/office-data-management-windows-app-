using System.Collections.Generic;
using System.Windows;
using UniversalOfficeDataManager.Models;
using UniversalOfficeDataManager.Repositories;
using UniversalOfficeDataManager.Views;

namespace UniversalOfficeDataManager.Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowWorkspaceEditor(Workspace workspace)
        {
            var window = new WorkspaceEditWindow(workspace) { Owner = Application.Current.MainWindow };
            return window.ShowDialog();
        }

        public bool? ShowSheetEditor(Sheet sheet)
        {
            var window = new SheetEditWindow(sheet) { Owner = Application.Current.MainWindow };
            return window.ShowDialog();
        }

        public bool? ShowFieldManager(Sheet sheet, IFieldRepository fieldRepository)
        {
            var window = new FieldManagerWindow(sheet, fieldRepository) { Owner = Application.Current.MainWindow };
            return window.ShowDialog();
        }

        public bool? ShowRecordEditor(List<FieldDefinition> fields, RecordItem record)
        {
            var window = new RecordEditWindow(fields, record) { Owner = Application.Current.MainWindow };
            return window.ShowDialog();
        }

        public bool ConfirmDelete(string message)
        {
            return MessageBox.Show(message, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                   == MessageBoxResult.Yes;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Universal Office Data Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
