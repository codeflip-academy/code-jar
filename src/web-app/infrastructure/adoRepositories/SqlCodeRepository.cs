using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using CodeJar.Domain;
using CodeJar.Infrastructure.Guids;

namespace CodeJar.Infrastructure
{
    public class SqlCodeRepository : ICodeRepository
    {
        private readonly SqlConnection _connection;

        public SqlCodeRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task AddAsync(IAsyncEnumerable<GeneratedCode> codes)
        {
            try
            {
                await _connection.OpenAsync();

                using(var command = _connection.CreateCommand())
                {
                    command.CommandText = $@"INSERT INTO Codes (BatchID, SeedValue, State) VALUES (@batchID, @seedValue, 0)";

                    command.Parameters.AddWithValue("@batchID", null);
                    command.Parameters.AddWithValue("@seedValue", null);

                    await foreach(var code in codes)
                    {
                        command.Parameters["@batchID"].Value = code.BatchId;
                        command.Parameters["@seedValue"].Value = code.SeedValue;

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            
            finally
            {
                await _connection.CloseAsync();
            }
        }
        
        public async Task UpdateAsync(List<Code> codes)
        {
            try
            {
                await _connection.OpenAsync();
                
                using(var command = _connection.CreateCommand())
                {
                    foreach(var code in codes)
                    {
                        command.Parameters.Clear();
                        command.CommandText = $@"UPDATE Codes SET State = @state WHERE ID = @id";

                        command.Parameters.AddWithValue("@state", CodeStateSerializer.SerializeState(code.State));
                        command.Parameters.AddWithValue("@id", code.Id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }
        
        public async Task UpdateAsync(Code code, Guid? id)
        {
            try
            {
                await _connection.OpenAsync();
                
                using(var command = _connection.CreateCommand())
                {
                    command.CommandText = $@"UPDATE Codes
                                             SET State = @state,
                                             DeactivatedBy = @deactivatedBy,
                                             DeactivatedOn = @deactivatedOn,
                                             RedeemedBy = @redeemedBy,
                                             RedeemedOn = @redeemedOn,
                                             RedemptionID = @redeemId
                                             WHERE ID = @id";

                    var deactivatedBy = code is DeactivatingCode ? (object) ((DeactivatingCode)code).By : DBNull.Value;
                    var deactivatedOn = code is DeactivatingCode ? (object) ((DeactivatingCode)code).When : DBNull.Value;
                    var redeemedBy = code is RedeemingCode ? (object) ((RedeemingCode)code).By : DBNull.Value;
                    var redeemedOn = code is RedeemingCode ? (object) ((RedeemingCode)code).When : DBNull.Value;
                    var redeemId = id;

                    command.Parameters.AddWithValue("@state", CodeStateSerializer.SerializeState(code.State));
                    command.Parameters.AddWithValue("@deactivatedBy", deactivatedBy);
                    command.Parameters.AddWithValue("@deactivatedOn", deactivatedOn);
                    command.Parameters.AddWithValue("@redeemedBy", redeemedBy);
                    command.Parameters.AddWithValue("@redeemedOn", redeemedOn);
                    command.Parameters.AddWithValue("@redeemId", redeemId);

                    command.Parameters.AddWithValue("@id", code.Id);

                    await command.ExecuteNonQueryAsync();
                }
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<RedeemingCode> GetRedeemingAsync(int value)
        {
            try
            {
                await _connection.OpenAsync();
                
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Codes.ID, Codes.State FROM Codes
                                            WHERE Codes.SeedValue = @seedValue AND Codes.State = 1";

                    command.Parameters.AddWithValue("@seedValue", value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var code = new RedeemingCode
                            {
                                Id = (int) reader["ID"]
                            };

                            code.GetType()
                            .GetProperty(nameof(code.State))
                            .SetValue(code, CodeStateSerializer.DeserializeState((byte) reader["State"]));

                            return code;
                        }
                    }
                }

                return new RedeemingCode();
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<DeactivatingCode> GetDeactivatingAsync(int value)
        {
            try
            {
                await _connection.OpenAsync();

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Codes.ID, Codes.State FROM Codes
                                            WHERE Codes.SeedValue = @seedValue AND (Codes.State = 0 OR Codes.State = 1)";
                    command.Parameters.AddWithValue("@seedValue", value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var code = new DeactivatingCode
                            {
                                Id = (int) reader["Id"]
                            };

                            code.GetType()
                            .GetProperty(nameof(code.State))
                            .SetValue(code, CodeStateSerializer.DeserializeState((byte) reader["State"]));

                            return code;
                        }
                    }
                }

                return new DeactivatingCode();
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async IAsyncEnumerable<ActivatingCode> GetActivatingAsync(DateTime date)
        {
            try
            {
                await _connection.OpenAsync();

                using(var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Codes.ID, Codes.State FROM Codes
                                            INNER JOIN Batch ON Batch.ID = Codes.BatchID
                                            WHERE Codes.State = 0 AND Batch.DateActive <= @date";

                    command.Parameters.AddWithValue("@date", date);

                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            var code = new ActivatingCode();
                            code.Id = (int) reader["ID"];

                            code.GetType()
                            .GetProperty(nameof(code.State))
                            .SetValue(code, CodeStateSerializer.DeserializeState((byte) reader["State"]));

                            yield return code;
                        }
                    }
                }
            }

            finally
            {
                await _connection.CloseAsync();
            }

        }

        public async IAsyncEnumerable<ExpiringCode> GetExpiringAsync(DateTime date)
        {
            try
            {
                await _connection.OpenAsync();

                using(var command = _connection.CreateCommand())
                {
                    command.CommandText = @"SELECT Codes.ID, Codes.State FROM Codes
                                            INNER JOIN Batch ON Batch.ID = Codes.BatchID
                                            WHERE (Codes.State = 0 OR Codes.State = 1) AND Batch.DateExpires <= @forDate";

                    command.Parameters.AddWithValue("@forDate", date.Date);

                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            var code = new ExpiringCode();
                            code.Id = (int) reader["ID"];
                            code.GetType()
                            .GetProperty(nameof(code.State))
                            .SetValue(code, CodeStateSerializer.DeserializeState((byte) reader["State"]));

                            yield return code;
                        }
                    }
                }
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}