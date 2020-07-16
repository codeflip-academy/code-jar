using System;

namespace CodeJar.Domain
{
    public static class CodeStates
    {
        public static readonly string Generated = "Generated";
        public static readonly string Active = "Active";
        public static readonly string Expired = "Expired";
        public static readonly string Redeemed = "Redeemed";
        public static readonly string Inactive = "Inactive";
    }
}