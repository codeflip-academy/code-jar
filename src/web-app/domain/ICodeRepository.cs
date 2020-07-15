using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CodeJar.Domain
{
    public interface ICodeRepository
    {
        Task UpdateAsync(List<Code> codes);
        Task UpdateAsync(Code code);
        Task AddAsync(IAsyncEnumerable<GeneratedCode> codes);
        Task<RedeemingCode> GetRedeemingAsync(int value);
        Task<DeactivatingCode> GetDeactivatingAsync(int value);
        IAsyncEnumerable<ActivatingCode> GetActivatingAsync(DateTime date);
        IAsyncEnumerable<ExpiringCode> GetExpiringAsync(DateTime date);
    }
}