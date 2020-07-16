using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeJar.Domain
{
    public interface ISeedValueReader
    {
        Task<List<int>> ReadSeedValuesAsync(int count);
    }
}