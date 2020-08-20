using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodeJar.Domain
{
    public abstract class Code
    {
        public int Id { get; set; }
        public string State { get; protected set; }
        public Guid RedemptionID { get; set; }
        public Guid BatchId { get; set; }
    }

    public class GeneratedCode : Code
    {
        public GeneratedCode(Guid batchId, int seedValue)
        {
            BatchId = batchId;
            SeedValue = seedValue;
            State = CodeStates.Generated;
        }

        public Guid BatchId { get; set; }
        public int SeedValue { get; set; }
    }

    public class DeactivatingCode : Code
    {
        public DateTime? When { get; private set; }
        public string By { get; private set; }

        public void Deactivate(string by, DateTime when)
        {
            By = by;
            When = when;
            base.State = CodeStates.Inactive;
        }
    }

    public class RedeemingCode : Code
    {
        public DateTime? When { get; private set; }
        public string By { get; private set; }
        public Guid RedeemId { get; private set; }
        public void Redeem(string by, DateTime when)
        {
            By = by;
            When = when;
            base.State = CodeStates.Redeemed;
            RedeemId = Guid.NewGuid();
        }
    }

    public class ExpiringCode : Code
    {
        public void Expire()
        {
            base.State = CodeStates.Expired;
        }
    }

    public class ActivatingCode : Code
    {
        public void Activate()
        {
            base.State = CodeStates.Active;
        }
    }
}
