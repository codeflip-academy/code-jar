using System;
using System.Data.SqlClient;

namespace CodeJar.Infrastructure
{
    public class Offset
    {
        private readonly SqlConnection _connection;

        public Offset(SqlConnection connection)
        {
            _connection = connection;
        }
        public (long, long) UpdateOffset(int count)
        {
            long start;
            long end;

            using(var command = _connection.CreateCommand())
            {
                command.CommandText = @"UPDATE Offset
                                   SET OffsetValue = OffsetValue + @offsetIncrement
                                   OUTPUT INSERTED.OffsetValue
                                   WHERE ID = 1";

                command.Parameters.AddWithValue("@offsetIncrement", count * 4);
                
                end = (long)command.ExecuteScalar();

                start = end - count * 4;

            }

            return (start, end);
        }
    }
}