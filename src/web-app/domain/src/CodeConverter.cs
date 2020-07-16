using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeJar.Domain
{
    public static class CodeConverter
    {
        public static string ConvertToCode(int seedvalue, string alphabet, int codeLength)
        {
            if(alphabet.Length < 2)
                throw new ArgumentException("Alphabet must be greater than 1.");

            var result = EncodeToBaseString(seedvalue, alphabet);

            result = result.PadLeft(codeLength, alphabet[0]);

            return result;
        }

        public static int ConvertFromCode(string code, string alphabet)
        {
            var result = DecodeFromBaseString(code, alphabet);

            return result;
        }

        private static string EncodeToBaseString(int seedvalue, string alphabet)
        {
            var encBase = alphabet.Length;

            var digits = "";
            var num = seedvalue;

            if (num == 0)
                return alphabet[0].ToString();

            while (num > 0)
            {
                digits = alphabet[num % encBase] + digits;
                num = num / encBase;
            }

            return digits;
        }
        
        private static int DecodeFromBaseString(string value, string alphabet)
        {
            var result = 0;

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[value.Length - 1 - i];
                var index = alphabet.IndexOf(c);
                var p = index * (int)Math.Pow(alphabet.Length, i);
                
                result = result + p;
            }

            return result;
        }
    }
}