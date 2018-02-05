// Copyright (c) 2018 Janis Kirsteins.
// Licenced under the MIT licence.
// See the LICENSE file in the project root for more information.

namespace DemoWallet
{
    using System;
    using System.Numerics;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents information about an Ethereum transaction.
    /// </summary>
    internal struct Transaction
    {
        /// <summary>
        /// Information about the block, in which this transaction was included.
        /// </summary>
        public Block Block;

        /// <summary>
        /// The transaction hash.
        /// </summary>
        public string TxHash;

        /// <summary>
        /// The address of the sender of this transaction (hex-string).
        /// </summary>
        public string From;

        /// <summary>
        /// The address of the recipient of this transaction (hex-string).
        /// </summary>
        public string To;

        /// <summary>
        /// The wei-denominated amount which was sent using this transaction (not including fees).
        /// </summary>
        public BigInteger AmountInWei;

        /// <summary>
        /// The wei-denominated amount which was paid in fees for this transaction.
        ///
        /// I.e. the transaction's spent gas value multiplied by the gas price value.
        /// </summary>
        public BigInteger FeeInWei;

        /// <summary>
        /// Gets an ether-denominated value representing the transaction amount.
        /// </summary>
        /// <returns>A decimal ether-denominated value.</returns>
        [JsonIgnore]
        internal decimal AmountInEth => this.AmountInWei.WeiToEth();

        /// <summary>
        /// Gets an ether-denominated value representing the transaction fee.
        /// </summary>
        /// <returns>A decimal ether-denominated value.</returns>
        [JsonIgnore]
        internal decimal FeeInEth => this.FeeInWei.WeiToEth();
    }
}