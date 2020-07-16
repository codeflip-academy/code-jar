using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CodeJar.Domain;

namespace CodeJar.Infrastructure
{
    public class SqlBatchRepository : IBatchRepository
    {
        private SqlConnection _connection;

        public SqlBatchRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task AddAsync(Batch batch)
        {
            try
            {
                await _connection.OpenAsync();
                
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"
                    INSERT INTO Batch (ID, BatchName, BatchSize, DateActive, DateExpires)
                    VALUES(@Id, @batchName, @batchSize, @dateActive, @dateExpires)";

                    command.Parameters.AddWithValue("@batchName", batch.BatchName);
                    command.Parameters.AddWithValue("@batchSize", batch.BatchSize);
                    command.Parameters.AddWithValue("@dateActive", batch.DateActive);
                    command.Parameters.AddWithValue("@dateExpires", batch.DateExpires);
                    command.Parameters.AddWithValue("@Id", batch.Id);

                    await command.ExecuteNonQueryAsync();
                }
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<Batch> GetBatchAsync(Guid id)
        {
            try
            {
                await _connection.OpenAsync();

                var batch = new Batch();
                
                using(var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM Batch WHERE ID = @id";

                    command.Parameters.AddWithValue("@id", id);
                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            batch.Id = (Guid) reader["ID"];
                            batch.BatchName = (string)reader["BatchName"];
                            batch.BatchSize = (int)reader["BatchSize"];
                            batch.DateActive = (DateTime)reader["DateActive"];
                            batch.DateExpires = (DateTime)reader["DateExpires"];
                        }
                    }
                    
                    return batch;
                }                
            }

            finally
            {
                await _connection.CloseAsync();
            }

        }

        public async Task<List<Batch>> GetBatchesAsync()
        {
            try
            {
                await _connection.OpenAsync();
                
                var batches = new List<Batch>();

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT * FROM Batch";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var batch = new Batch
                            {
                                Id = (Guid)reader["ID"],
                                BatchName = (string)reader["BatchName"],
                                BatchSize = (int)reader["BatchSize"],
                                DateActive = (DateTime)reader["DateActive"],
                                DateExpires = (DateTime)reader["DateExpires"],
                            };

                            batches.Add(batch);
                        }
                    }
                }

                return batches;
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task DeactivateBatchAsync(Guid batchId)
        {
            try
            {
                await _connection.OpenAsync();

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE Codes SET [State] = @inactive
                                            WHERE BatchID = @batchId AND State = 1";

                    command.Parameters.AddWithValue("@inactive", CodeStates.Inactive);
                    command.Parameters.AddWithValue("@batchId", batchId);
                    await command.ExecuteNonQueryAsync();
                }                
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}