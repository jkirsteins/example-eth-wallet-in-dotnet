using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;

namespace wallet {
    static class DecimalExtensions {
        public static BigInteger EthToWei(this decimal val) {
            return BigInteger.Parse((val * 1E18M).ToString());
        }
    }
}