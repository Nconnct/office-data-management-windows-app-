using System;
using System.Windows;
using UniversalOfficeDataManager.Data;
using UniversalOfficeDataManager.Repositories;
using UniversalOfficeDataManager.Services;
using UniversalOfficeDataManager.ViewModels;

namespace UniversalOfficeDataManager
{
    /// <summary>
    /// Application entry point. Initializes the local SQLite database and
    /// wires up the (few) dependencies the app needs by hand — no DI
    /// container yet, since Stage 1's object graph is small enough that a
    /// container would add ceremony without real benefit. This can be
    /// revisited once the number of services grows in later stages.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                DatabaseInitializer.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not initialize the local database at:\n{DatabasePathProvider.GetDatabaseFilePath()}\n\n{ex.Message}",
                    "Universal Office Data Manager — Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(-1);
                return;
            }

            IWorkspaceRepository workspaceRepository = new WorkspaceRepository();
            ISheetRepository sheetRepository = new SheetRepository();
            IFieldRepository fieldRepository = new FieldRepository();
            IRecordRepository recordRepository = new RecordRepository();
            IDialogService dialogService = new DialogService();

            var mainViewModel = new MainViewModel(
                workspaceRepository,
                sheetRepository,
                fieldRepository,
                recordRepository,
                dialogService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }
    }
}
