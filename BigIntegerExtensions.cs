// Copyright (c) 2018 Janis Kirsteins.
// Licenced under the MIT licence.
// See the LICENSE file in the project root for more information.

namespace DemoWallet
{
    using System.Numerics;
    using Nethereum.Hex.HexConvertors.Extensions;

    /// <summary>
    /// Provides extension methods for converting between different denominations and
    /// types.
    /// </summary>
    internal static class BigIntegerExtensions
    {
        /// <summary>
        /// Convert a BigInteger wei-denominated value to
        /// an ether-denominated decimal value.
        /// </summary>
        /// <param name="val">wei-denominated value</param>
        /// <returns>ether-denominated value</returns>
        internal static decimal WeiToEth(this BigInteger val)
        {
            return decimal.Parse(val.ToString()) / 1E18M;
        }

        /// <summary>
        /// Convert a BigInteger gwei-denominated value to a
        /// BigInteger wei-denominated value.
        /// In other words - adds 9 zeros.
        /// </summary>
        /// <param name="val">gwei-denominated value</param>
        /// <returns>wei-denominated value</returns>
        internal static BigInteger GweiToWei(this BigInteger val)
        {
            return BigInteger.Parse(string.Format("{0}000000000", val));
        }
    }
}