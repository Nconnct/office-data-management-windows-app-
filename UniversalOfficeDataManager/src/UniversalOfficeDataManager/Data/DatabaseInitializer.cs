using Microsoft.Data.Sqlite;

namespace UniversalOfficeDataManager.Data
{
    /// <summary>
    /// Creates office_manager.db and its schema on first run. All CREATE
    /// statements are idempotent (IF NOT EXISTS) so calling this on every
    /// startup is safe and cheap.
    ///
    /// Values are stored as a generic RecordValues(RecordId, FieldId, Value)
    /// table rather than one physical column per field, because sheets and
    /// their fields are fully user-defined and change at runtime. This also
    /// gives later stages (formulas, lookups) a stable place to read from
    /// without another schema migration.
    /// </summary>
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            connection.Open();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            using var transaction = connection.BeginTransaction();
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = SchemaScript;
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        private const string SchemaScript = @"
CREATE TABLE IF NOT EXISTS Workspaces (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    ColorHex TEXT NOT NULL DEFAULT '#2563EB',
    CreatedAt TEXT NOT NULL,
    ModifiedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Sheets (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkspaceId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT,
    CreatedAt TEXT NOT NULL,
    ModifiedAt TEXT NOT NULL,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_Sheets_WorkspaceId ON Sheets(WorkspaceId);

CREATE TABLE IF NOT EXISTS Fields (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SheetId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    FieldType TEXT NOT NULL,
    OrderIndex INTEGER NOT NULL DEFAULT 0,
    IsRequired INTEGER NOT NULL DEFAULT 0,
    DefaultValue TEXT,
    OptionsJson TEXT,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (SheetId) REFERENCES Sheets(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_Fields_SheetId ON Fields(SheetId);

CREATE TABLE IF NOT EXISTS Records (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SheetId INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    ModifiedAt TEXT NOT NULL,
    FOREIGN KEY (SheetId) REFERENCES Sheets(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_Records_SheetId ON Records(SheetId);

CREATE TABLE IF NOT EXISTS RecordValues (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RecordId INTEGER NOT NULL,
    FieldId INTEGER NOT NULL,
    Value TEXT,
    FOREIGN KEY (RecordId) REFERENCES Records(Id) ON DELETE CASCADE,
    FOREIGN KEY (FieldId) REFERENCES Fields(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS UX_RecordValues_Record_Field ON RecordValues(RecordId, FieldId);
CREATE INDEX IF NOT EXISTS IX_RecordValues_FieldId ON RecordValues(FieldId);

CREATE TABLE IF NOT EXISTS AppSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT
);
";
    }
}
