using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodeJar.Domain
{

    public class Batch
    {
        public Guid Id {get; set;}
        public string BatchName {get; set;}
        public int BatchSize {get; set;}
        public DateTime DateActive {get; set;}
        public DateTime DateExpires {get; set;}

        public async IAsyncEnumerable<GeneratedCode> GenerateCodesAsync(ISeedValueReader reader)
        {
            foreach(var seedValue in await reader.ReadSeedValuesAsync(BatchSize))
                yield return new GeneratedCode(Id, seedValue);
        }
    }
}