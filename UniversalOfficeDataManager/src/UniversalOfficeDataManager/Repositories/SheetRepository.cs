using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using UniversalOfficeDataManager.Data;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Repositories
{
    public interface ISheetRepository
    {
        Task<List<Sheet>> GetByWorkspaceAsync(int workspaceId);
        Task<int> CountAllAsync();
        Task<Sheet> CreateAsync(Sheet sheet);
        Task UpdateAsync(Sheet sheet);
        Task DeleteAsync(int sheetId);
    }

    public class SheetRepository : ISheetRepository
    {
        public async Task<List<Sheet>> GetByWorkspaceAsync(int workspaceId)
        {
            var results = new List<Sheet>();

            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, WorkspaceId, Name, Description, CreatedAt, ModifiedAt
                FROM Sheets
                WHERE WorkspaceId = $workspaceId
                ORDER BY Name COLLATE NOCASE;";
            command.Parameters.AddWithValue("$workspaceId", workspaceId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new Sheet
                {
                    Id = reader.GetInt32(0),
                    WorkspaceId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4)),
                    ModifiedAt = DateTime.Parse(reader.GetString(5))
                });
            }

            return results;
        }

        public async Task<int> CountAllAsync()
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Sheets;";
            var result = await command.ExecuteScalarAsync();
            return result is long count ? (int)count : 0;
        }

        public async Task<Sheet> CreateAsync(Sheet sheet)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            var now = DateTime.UtcNow;
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Sheets (WorkspaceId, Name, Description, CreatedAt, ModifiedAt)
                VALUES ($workspaceId, $name, $description, $createdAt, $modifiedAt);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$workspaceId", sheet.WorkspaceId);
            command.Parameters.AddWithValue("$name", sheet.Name);
            command.Parameters.AddWithValue("$description", (object?)sheet.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$createdAt", now.ToString("O"));
            command.Parameters.AddWithValue("$modifiedAt", now.ToString("O"));

            var newId = (long)(await command.ExecuteScalarAsync() ?? 0L);
            sheet.Id = (int)newId;
            sheet.CreatedAt = now;
            sheet.ModifiedAt = now;
            return sheet;
        }

        public async Task UpdateAsync(Sheet sheet)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            var now = DateTime.UtcNow;
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Sheets
                SET Name = $name, Description = $description, ModifiedAt = $modifiedAt
                WHERE Id = $id;";
            command.Parameters.AddWithValue("$name", sheet.Name);
            command.Parameters.AddWithValue("$description", (object?)sheet.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$modifiedAt", now.ToString("O"));
            command.Parameters.AddWithValue("$id", sheet.Id);
            await command.ExecuteNonQueryAsync();

            sheet.ModifiedAt = now;
        }

        public async Task DeleteAsync(int sheetId)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Sheets WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", sheetId);
            await command.ExecuteNonQueryAsync();
        }
    }
}
