using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using UniversalOfficeDataManager.Data;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Repositories
{
    public interface IWorkspaceRepository
    {
        Task<List<Workspace>> GetAllAsync();
        Task<int> CountAsync();
        Task<Workspace> CreateAsync(Workspace workspace);
        Task UpdateAsync(Workspace workspace);
        Task DeleteAsync(int workspaceId);
    }

    public class WorkspaceRepository : IWorkspaceRepository
    {
        public async Task<List<Workspace>> GetAllAsync()
        {
            var results = new List<Workspace>();

            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, ColorHex, CreatedAt, ModifiedAt
                FROM Workspaces
                ORDER BY Name COLLATE NOCASE;";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new Workspace
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ColorHex = reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4)),
                    ModifiedAt = DateTime.Parse(reader.GetString(5))
                });
            }

            return results;
        }

        public async Task<int> CountAsync()
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Workspaces;";
            var result = await command.ExecuteScalarAsync();
            return result is long count ? (int)count : 0;
        }

        public async Task<Workspace> CreateAsync(Workspace workspace)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            var now = DateTime.UtcNow;
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Workspaces (Name, Description, ColorHex, CreatedAt, ModifiedAt)
                VALUES ($name, $description, $colorHex, $createdAt, $modifiedAt);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$name", workspace.Name);
            command.Parameters.AddWithValue("$description", (object?)workspace.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$colorHex", workspace.ColorHex);
            command.Parameters.AddWithValue("$createdAt", now.ToString("O"));
            command.Parameters.AddWithValue("$modifiedAt", now.ToString("O"));

            var newId = (long)(await command.ExecuteScalarAsync() ?? 0L);
            workspace.Id = (int)newId;
            workspace.CreatedAt = now;
            workspace.ModifiedAt = now;
            return workspace;
        }

        public async Task UpdateAsync(Workspace workspace)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            var now = DateTime.UtcNow;
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Workspaces
                SET Name = $name, Description = $description, ColorHex = $colorHex, ModifiedAt = $modifiedAt
                WHERE Id = $id;";
            command.Parameters.AddWithValue("$name", workspace.Name);
            command.Parameters.AddWithValue("$description", (object?)workspace.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$colorHex", workspace.ColorHex);
            command.Parameters.AddWithValue("$modifiedAt", now.ToString("O"));
            command.Parameters.AddWithValue("$id", workspace.Id);
            await command.ExecuteNonQueryAsync();

            workspace.ModifiedAt = now;
        }

        public async Task DeleteAsync(int workspaceId)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Workspaces WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", workspaceId);
            await command.ExecuteNonQueryAsync();
        }
    }
}
