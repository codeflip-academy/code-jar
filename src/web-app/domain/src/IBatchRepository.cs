using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeJar.Domain
{
    public interface IBatchRepository
    {
        Task<List<Batch>> GetBatchesAsync();
        Task AddAsync(Batch batch);
        Task<Batch> GetBatchAsync(Guid id);
        Task DeactivateBatchAsync(Guid id);
    }
}