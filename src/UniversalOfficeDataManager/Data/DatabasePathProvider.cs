using System;
using System.IO;

namespace UniversalOfficeDataManager.Data
{
    /// <summary>
    /// Resolves where office_manager.db lives on disk. Using LocalAppData
    /// (rather than the install folder) avoids permission problems when
    /// the app is installed under Program Files, and keeps user data
    /// separate from the application binaries.
    /// </summary>
    public static class DatabasePathProvider
    {
        private const string FolderName = "UniversalOfficeDataManager";
        private const string DatabaseFileName = "office_manager.db";

        public static string GetDatabaseFolder()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                FolderName);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static string GetDatabaseFilePath() =>
            Path.Combine(GetDatabaseFolder(), DatabaseFileName);

        public static string GetConnectionString() =>
            $"Data Source={GetDatabaseFilePath()}";
    }
}
