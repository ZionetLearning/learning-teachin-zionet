using AzureFunctionsProject.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;

namespace AzureFunctionsProject.Services
{
    public class DataService : IDataService
    {
        private readonly Func<NpgsqlConnection> _dbFactory;
        private readonly ILogger<DataService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public DataService(Func<NpgsqlConnection> dbFactory, ILogger<DataService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _retryPolicy = Policy
                .Handle<NpgsqlException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, delay, retryCount, ctx) =>
                        _logger.LogWarning(ex,
                            "DB transient error, retry {RetryCount} in {Delay}s",
                            retryCount, delay.TotalSeconds)
                );
            // Ensure table once at startup
            _ = EnsureTableExistsAsync(CancellationToken.None);
        }

        private Task EnsureTableExistsAsync(CancellationToken ct = default) =>
            _retryPolicy.ExecuteAsync(async token =>
            {
                using var conn = _dbFactory();
                await conn.OpenAsync(token);
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS data (
                        id UUID PRIMARY KEY,
                        payload TEXT NOT NULL
                    );";
                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync(token);
            }, ct);

        public Task<List<DataDto>> GetAllAsync(CancellationToken ct = default) =>
            _retryPolicy.ExecuteAsync(async token =>
            {
                _logger.LogInformation("GetAllAsync starting");
                await using var conn = _dbFactory();
                await conn.OpenAsync(token);

                await using var cmd = new NpgsqlCommand(
                    "SELECT id, payload, xmin AS version FROM data", conn);
                await using var reader = await cmd.ExecuteReaderAsync(token);

                var list = new List<DataDto>();
                while (await reader.ReadAsync(token))
                {
                    list.Add(new DataDto
                    {
                        Id = reader.GetFieldValue<Guid>(0),
                        Payload = reader.GetString(1),
                        Version = reader.GetFieldValue<uint>(2)
                    });
                }
                _logger.LogInformation("GetAllAsync returned {Count} items", list.Count);
                return list;
            }, ct);

        public Task<DataDto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _retryPolicy.ExecuteAsync(async token =>
            {
                _logger.LogInformation("GetByIdAsync starting for {Id}", id);
                await using var conn = _dbFactory();
                await conn.OpenAsync(token);

                await using var cmd = new NpgsqlCommand(
                    "SELECT id, payload, xmin AS version FROM data WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("id", id);

                await using var reader = await cmd.ExecuteReaderAsync(token);
                if (!await reader.ReadAsync(token))
                {
                    _logger.LogWarning("GetByIdAsync found no record for {Id}", id);
                    return null;
                }

                var dto = new DataDto
                {
                    Id = reader.GetFieldValue<Guid>(0),
                    Payload = reader.GetString(1),
                    Version = reader.GetFieldValue<uint>(2)
                };
                _logger.LogInformation("GetByIdAsync retrieved {Id}@v{Version}", dto.Id, dto.Version);
                return dto;
            }, ct);

        public Task CreateAsync(DataDto entity, CancellationToken ct = default) =>
            _retryPolicy.ExecuteAsync(async token =>
            {
                _logger.LogInformation("CreateAsync inserting {Id}", entity.Id);
                await using var conn = _dbFactory();
                await conn.OpenAsync(token);

                await using var cmd = new NpgsqlCommand(
                    "INSERT INTO data(id, payload) VALUES(@id, @payload)", conn);
                cmd.Parameters.AddWithValue("id", entity.Id);
                cmd.Parameters.AddWithValue("payload", entity.Payload);

                await cmd.ExecuteNonQueryAsync(token);
                _logger.LogInformation("CreateAsync succeeded for {Id}", entity.Id);
            }, ct);

        public Task UpdateAsync(DataDto entity, CancellationToken ct = default) =>
            _retryPolicy.ExecuteAsync(async token =>
            {
                _logger.LogInformation("UpdateAsync for {Id}@v{Version}", entity.Id, entity.Version);
                await using var conn = _dbFactory();
                await conn.OpenAsync(token);

                // Enforce optimistic concurrency via xmin
                await using var cmd = new NpgsqlCommand(
                    @"UPDATE data 
                      SET payload = @payload 
                      WHERE id = @id AND xmin = @version", conn);
                cmd.Parameters.AddWithValue("id", entity.Id);
                cmd.Parameters.AddWithValue("payload", entity.Payload);
                cmd.Parameters.AddWithValue("version", (long)entity.Version);

                var rows = await cmd.ExecuteNonQueryAsync(token);
                if (rows == 0)
                    throw new InvalidOperationException($"Concurrent update conflict for {entity.Id}");

                _logger.LogInformation("UpdateAsync succeeded for {Id}", entity.Id);
            }, ct);

        public Task DeleteAsync(Guid id, CancellationToken ct = default) =>
            _retryPolicy.ExecuteAsync(async token =>
            {
                _logger.LogInformation("DeleteAsync for {Id}", id);
                await using var conn = _dbFactory();
                await conn.OpenAsync(token);

                await using var cmd = new NpgsqlCommand(
                    "DELETE FROM data WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("id", id);

                await cmd.ExecuteNonQueryAsync(token);
                _logger.LogInformation("DeleteAsync succeeded for {Id}", id);
            }, ct);
    }
}
