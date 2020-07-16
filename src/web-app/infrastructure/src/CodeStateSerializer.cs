using System;
using System.Collections.Generic;
using System.Text;
using CodeJar.Domain;

namespace CodeJar.Infrastructure
{
    public static class CodeStateSerializer
    {
        public static string DeserializeState(byte state)
        {
            switch(state)
            {
                case 0:
                    return CodeStates.Generated;
                case 1:
                    return CodeStates.Active;
                case 2:
                    return CodeStates.Expired;
                case 3:
                    return CodeStates.Redeemed;
                case 4:
                    return CodeStates.Inactive;
                default:
                    throw new ArgumentException("invalid state value.");
            }
        }

        public static byte SerializeState(string state)
        {
            if (state == CodeStates.Generated)
                return 0;

            else if (state == CodeStates.Active)
                return 1;
            
            else if (state == CodeStates.Expired)
                return 2;
            
            else if (state == CodeStates.Redeemed)
                return 3;

            else if (state == CodeStates.Inactive)
                return 4;
            
            else
                throw new ArgumentException("invalid state value.");
        }
    }
    
}
 