using System;
using Nethereum.Hex.HexConvertors.Extensions;

namespace wallet {
    static class StringExtensions {
        public static bool IsValidEthereumAddress(this string val) {
            try {
                return val.HexToByteArray().Length == 20;
            } catch (InvalidOperationException) {
                return false;
            }
        }
    }
}