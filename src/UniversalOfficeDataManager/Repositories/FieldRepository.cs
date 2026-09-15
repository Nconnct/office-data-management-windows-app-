using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using UniversalOfficeDataManager.Data;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Repositories
{
    public interface IFieldRepository
    {
        Task<List<FieldDefinition>> GetBySheetAsync(int sheetId);
        Task<FieldDefinition> CreateAsync(FieldDefinition field);
        Task UpdateAsync(FieldDefinition field);
        Task DeleteAsync(int fieldId);
    }

    public class FieldRepository : IFieldRepository
    {
        public async Task<List<FieldDefinition>> GetBySheetAsync(int sheetId)
        {
            var results = new List<FieldDefinition>();

            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, SheetId, Name, FieldType, OrderIndex, IsRequired, DefaultValue, OptionsJson, CreatedAt
                FROM Fields
                WHERE SheetId = $sheetId
                ORDER BY OrderIndex, Id;";
            command.Parameters.AddWithValue("$sheetId", sheetId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var optionsJson = reader.IsDBNull(7) ? null : reader.GetString(7);

                results.Add(new FieldDefinition
                {
                    Id = reader.GetInt32(0),
                    SheetId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    FieldType = Enum.Parse<FieldType>(reader.GetString(3)),
                    OrderIndex = reader.GetInt32(4),
                    IsRequired = reader.GetInt64(5) == 1,
                    DefaultValue = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Options = string.IsNullOrWhiteSpace(optionsJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(optionsJson) ?? new List<string>(),
                    CreatedAt = DateTime.Parse(reader.GetString(8))
                });
            }

            return results;
        }

        public async Task<FieldDefinition> CreateAsync(FieldDefinition field)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            var now = DateTime.UtcNow;
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Fields (SheetId, Name, FieldType, OrderIndex, IsRequired, DefaultValue, OptionsJson, CreatedAt)
                VALUES ($sheetId, $name, $fieldType, $orderIndex, $isRequired, $defaultValue, $optionsJson, $createdAt);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$sheetId", field.SheetId);
            command.Parameters.AddWithValue("$name", field.Name);
            command.Parameters.AddWithValue("$fieldType", field.FieldType.ToString());
            command.Parameters.AddWithValue("$orderIndex", field.OrderIndex);
            command.Parameters.AddWithValue("$isRequired", field.IsRequired ? 1 : 0);
            command.Parameters.AddWithValue("$defaultValue", (object?)field.DefaultValue ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "$optionsJson",
                field.Options.Count > 0 ? JsonSerializer.Serialize(field.Options) : (object)DBNull.Value);
            command.Parameters.AddWithValue("$createdAt", now.ToString("O"));

            var newId = (long)(await command.ExecuteScalarAsync() ?? 0L);
            field.Id = (int)newId;
            field.CreatedAt = now;
            return field;
        }

        public async Task UpdateAsync(FieldDefinition field)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Fields
                SET Name = $name,
                    FieldType = $fieldType,
                    OrderIndex = $orderIndex,
                    IsRequired = $isRequired,
                    DefaultValue = $defaultValue,
                    OptionsJson = $optionsJson
                WHERE Id = $id;";
            command.Parameters.AddWithValue("$name", field.Name);
            command.Parameters.AddWithValue("$fieldType", field.FieldType.ToString());
            command.Parameters.AddWithValue("$orderIndex", field.OrderIndex);
            command.Parameters.AddWithValue("$isRequired", field.IsRequired ? 1 : 0);
            command.Parameters.AddWithValue("$defaultValue", (object?)field.DefaultValue ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "$optionsJson",
                field.Options.Count > 0 ? JsonSerializer.Serialize(field.Options) : (object)DBNull.Value);
            command.Parameters.AddWithValue("$id", field.Id);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int fieldId)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Fields WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", fieldId);
            await command.ExecuteNonQueryAsync();
        }
    }
}
