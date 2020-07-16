using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using CodeJar.Domain;

namespace CodeJar.Infrastructure
{
    public class FileSystemSeedValueReader : ISeedValueReader
    {
        private readonly string path;
        private readonly SqlConnection _connection;

        public FileSystemSeedValueReader(string path, SqlConnection connection)
        {
            this.path = path;
            _connection = connection;
        }

        public async Task<List<int>> ReadSeedValuesAsync(int count)
        {
            var list = new List<int>();
            var offset = new Offset(_connection);
            var startAndEnd = await offset.UpdateOffsetAsync(count);
            var start = startAndEnd.Item1;
            var end = startAndEnd.Item2;

            using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                // Get the next offset position
                if (start % 4 != 0)
                    throw new ArgumentException("Offset must be divisible by 4");

                // Loop to the last offset position
                for (var i = start; i < end; i += 4)
                {
                    // Set reader to offset position
                    reader.BaseStream.Position = i;
                    var seedValue = reader.ReadInt32();

                    list.Add(seedValue);
                }
            }

            return list;
        }
    }
}