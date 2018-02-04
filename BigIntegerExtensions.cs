using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;

namespace wallet {
    static class BigIntegerExtensions {
        public static decimal WeiToEth(this BigInteger val) {
            return decimal.Parse(val.ToString()) / 1E18M;
        }

        // Just add 9 zeros
        public static BigInteger GweiToWei(this BigInteger val) {
            return BigInteger.Parse(string.Format("{0}000000000", val));
        }
    }
}