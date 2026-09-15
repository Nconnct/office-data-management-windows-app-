using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using UniversalOfficeDataManager.Data;
using UniversalOfficeDataManager.Models;

namespace UniversalOfficeDataManager.Repositories
{
    public interface IRecordRepository
    {
        Task<List<RecordItem>> GetBySheetAsync(int sheetId);
        Task<int> CountAllAsync();
        Task<int> CountBySheetAsync(int sheetId);
        Task<RecordItem> CreateAsync(RecordItem record);
        Task UpdateAsync(RecordItem record);
        Task DeleteAsync(int recordId);
    }

    public class RecordRepository : IRecordRepository
    {
        public async Task<List<RecordItem>> GetBySheetAsync(int sheetId)
        {
            var records = new Dictionary<int, RecordItem>();

            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT Id, SheetId, CreatedAt, ModifiedAt
                    FROM Records
                    WHERE SheetId = $sheetId
                    ORDER BY Id;";
                command.Parameters.AddWithValue("$sheetId", sheetId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var record = new RecordItem
                    {
                        Id = reader.GetInt32(0),
                        SheetId = reader.GetInt32(1),
                        CreatedAt = DateTime.Parse(reader.GetString(2)),
                        ModifiedAt = DateTime.Parse(reader.GetString(3))
                    };
                    records[record.Id] = record;
                }
            }

            if (records.Count > 0)
            {
                using var valueCommand = connection.CreateCommand();
                valueCommand.CommandText = @"
                    SELECT rv.RecordId, rv.FieldId, rv.Value
                    FROM RecordValues rv
                    INNER JOIN Records r ON r.Id = rv.RecordId
                    WHERE r.SheetId = $sheetId;";
                valueCommand.Parameters.AddWithValue("$sheetId", sheetId);

                using var reader = await valueCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var recordId = reader.GetInt32(0);
                    var fieldId = reader.GetInt32(1);
                    var value = reader.IsDBNull(2) ? null : reader.GetString(2);

                    if (records.TryGetValue(recordId, out var record))
                        record.Values[fieldId] = value;
                }
            }

            return records.Values.OrderBy(r => r.Id).ToList();
        }

        public async Task<int> CountAllAsync()
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Records;";
            var result = await command.ExecuteScalarAsync();
            return result is long count ? (int)count : 0;
        }

        public async Task<int> CountBySheetAsync(int sheetId)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Records WHERE SheetId = $sheetId;";
            command.Parameters.AddWithValue("$sheetId", sheetId);
            var result = await command.ExecuteScalarAsync();
            return result is long count ? (int)count : 0;
        }

        public async Task<RecordItem> CreateAsync(RecordItem record)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var now = DateTime.UtcNow;
            long newId;

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO Records (SheetId, CreatedAt, ModifiedAt)
                    VALUES ($sheetId, $createdAt, $modifiedAt);
                    SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("$sheetId", record.SheetId);
                command.Parameters.AddWithValue("$createdAt", now.ToString("O"));
                command.Parameters.AddWithValue("$modifiedAt", now.ToString("O"));
                newId = (long)(await command.ExecuteScalarAsync() ?? 0L);
            }

            foreach (var kvp in record.Values)
            {
                using var valueCommand = connection.CreateCommand();
                valueCommand.Transaction = transaction;
                valueCommand.CommandText = @"
                    INSERT INTO RecordValues (RecordId, FieldId, Value)
                    VALUES ($recordId, $fieldId, $value);";
                valueCommand.Parameters.AddWithValue("$recordId", newId);
                valueCommand.Parameters.AddWithValue("$fieldId", kvp.Key);
                valueCommand.Parameters.AddWithValue("$value", (object?)kvp.Value ?? DBNull.Value);
                await valueCommand.ExecuteNonQueryAsync();
            }

            transaction.Commit();

            record.Id = (int)newId;
            record.CreatedAt = now;
            record.ModifiedAt = now;
            return record;
        }

        public async Task UpdateAsync(RecordItem record)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var now = DateTime.UtcNow;

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "UPDATE Records SET ModifiedAt = $modifiedAt WHERE Id = $id;";
                command.Parameters.AddWithValue("$modifiedAt", now.ToString("O"));
                command.Parameters.AddWithValue("$id", record.Id);
                await command.ExecuteNonQueryAsync();
            }

            foreach (var kvp in record.Values)
            {
                using var upsert = connection.CreateCommand();
                upsert.Transaction = transaction;
                upsert.CommandText = @"
                    INSERT INTO RecordValues (RecordId, FieldId, Value)
                    VALUES ($recordId, $fieldId, $value)
                    ON CONFLICT(RecordId, FieldId) DO UPDATE SET Value = excluded.Value;";
                upsert.Parameters.AddWithValue("$recordId", record.Id);
                upsert.Parameters.AddWithValue("$fieldId", kvp.Key);
                upsert.Parameters.AddWithValue("$value", (object?)kvp.Value ?? DBNull.Value);
                await upsert.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            record.ModifiedAt = now;
        }

        public async Task DeleteAsync(int recordId)
        {
            using var connection = new SqliteConnection(DatabasePathProvider.GetConnectionString());
            await connection.OpenAsync();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Records WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", recordId);
            await command.ExecuteNonQueryAsync();
        }
    }
}
