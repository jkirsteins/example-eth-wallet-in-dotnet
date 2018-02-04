using System;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;

namespace wallet {
    static class ConsoleEx {
        public static string PromptForAddress(string message) {
            string result;
            do {
                result = Prompt(message);
            } while (!result.IsValidEthereumAddress());

            return result;
        }
        public static decimal PromptForDecimal(string message) {
            decimal? result = null;
            do {
                var line = Prompt(message);
                
                decimal amt;
                if (decimal.TryParse(line, out amt)) {
                    result = amt;
                }
            } while (!result.HasValue);

            return result.Value;
        }
        
        public static string Prompt(string message) {
            Console.Write("{0}: ", message);
            return Console.ReadLine();
        }

        public static BigInteger PromptForBigInteger(string message, BigInteger defaultValue)
        {
            BigInteger? result = null;
            do {
                result = PromptForOptionalBigInteger(message, defaultValue);
            } while (!result.HasValue);
            
            return result.Value;
        }

        internal static BigInteger? PromptForOptionalBigInteger(string message, BigInteger? defaultValue)
        {
            BigInteger? result = null;
            do {
                var line = Prompt(message);
                if (string.IsNullOrWhiteSpace(line)) {
                    return defaultValue;
                } 
                
                BigInteger parsedValue;
                if (BigInteger.TryParse(line, out parsedValue)) {
                    result = parsedValue;
                }
            } while (!result.HasValue);
            
            return result.Value;
        }
    }
}