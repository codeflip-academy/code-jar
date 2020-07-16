using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CodeJar.Infrastructure
{
    public class PaginationCount
    {
        private readonly SqlConnection _connection;

        public PaginationCount(SqlConnection connection)
        {
            _connection = connection;
        }

         public async Task<int> PageCount(Guid id)
        {
            var pages = 0;

            var pagesRemainder = 0;

            try
            {
                await _connection.OpenAsync();

                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT BatchSize FROM Batch WHERE ID = @id";

                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var numberOfCodes = (int)reader["BatchSize"];

                            pages = numberOfCodes / 10;

                            pagesRemainder = numberOfCodes % 10;

                            if (pagesRemainder > 0)
                            {
                                pages++;
                            }
                        }
                    }
                }
                return pages;
            }

            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
   
}