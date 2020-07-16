using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using System.Threading;
using System.Threading.Tasks;
using CodeJar.Domain;
using System.Data.SqlClient;
using CodeJar.Infrastructure;

namespace CodeFlip.CodeJar.Api
{
    public class CloudReader : ISeedValueReader
    {
        private readonly SqlConnection _connection;

        public CloudReader(string filePath, SqlConnection connection)
        {
            FilePath = new Uri(filePath);
            _connection = connection;
        }

        public Uri FilePath { get; private set; }

        public async Task<List<int>> ReadSeedValuesAsync(int count)
        {
            var offset = new Offset(_connection);
            var startAndEnd = await offset.UpdateOffsetAsync(count);
            var start = startAndEnd.Item1;
            var end = startAndEnd.Item2;
            
            var file = new CloudBlockBlob(FilePath);
            var bytes = new byte[count* sizeof(int)];

            await file.DownloadRangeToByteArrayAsync(bytes, index: 0, blobOffset: start, length: bytes.Length);

            var list = new List<int>();

            for(var i = 0; i < bytes.Length; i += 4)
            {
                var seedValue = BitConverter.ToInt32(bytes, i);

                list.Add(seedValue);
            }

            return list;
        }
    }
}
