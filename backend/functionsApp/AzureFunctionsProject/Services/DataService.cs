using AzureFunctionsProject.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AzureFunctionsProject.Services
{
    public class DataService : IDataService
    {
        private readonly Func<NpgsqlConnection> _dbFactory;
        private readonly ILogger<DataService> _logger;

        public DataService(Func<NpgsqlConnection> dbFactory, ILogger<DataService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        /// <summary>
        /// Ensures the 'data' table exists.
        /// </summary>
        private async Task EnsureTableExistsAsync()
        {
            try
            {
                using var conn = _dbFactory();
                await conn.OpenAsync();

                var createSql = @"
                    CREATE TABLE IF NOT EXISTS data (
                        id UUID PRIMARY KEY,
                        payload TEXT NOT NULL
                    );";

                await using var cmd = new NpgsqlCommand(createSql, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring data table exists");
                throw;
            }
        }

        public async Task<List<DataDto>> GetAllAsync()
        {
            await EnsureTableExistsAsync();

            using var conn = _dbFactory();
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT id, payload, xmin AS version FROM data", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<DataDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new DataDto
                {
                    Id = reader.GetFieldValue<Guid>(0),
                    Payload = reader.GetString(1),
                    Version = reader.GetFieldValue<uint>(2)
                });
            }
            return list;
        }

        public async Task<DataDto?> GetByIdAsync(Guid id)
        {
            await EnsureTableExistsAsync();

            using var conn = _dbFactory();
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT id, payload, xmin AS version FROM data WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new DataDto
            {
                Id = reader.GetFieldValue<Guid>(0),
                Payload = reader.GetString(1),
                Version = reader.GetFieldValue<uint>(2)
            };
        }

        public async Task CreateAsync(DataDto entity)
        {
            await EnsureTableExistsAsync();

            using var conn = _dbFactory();
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO data(id, payload) VALUES(@id, @payload)", conn);
            cmd.Parameters.AddWithValue("id", entity.Id);
            cmd.Parameters.AddWithValue("payload", entity.Payload);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(DataDto entity)
        {
            await EnsureTableExistsAsync();

            using var conn = _dbFactory();
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "UPDATE data SET payload = @payload WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", entity.Id);
            cmd.Parameters.AddWithValue("payload", entity.Payload);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await EnsureTableExistsAsync();

            using var conn = _dbFactory();
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "DELETE FROM data WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
