// Copyright (c) 2018 Janis Kirsteins.
// Licenced under the MIT licence.
// See the LICENSE file in the project root for more information.

namespace DemoWallet
{
    using System;
    using System.Numerics;
    using Nethereum.Hex.HexTypes;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents information about an Ethereum transaction.
    /// </summary>
    internal struct Transaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> struct.
        /// </summary>
        /// <param name="block">A block object representing the block in which this transaction was included.</param>
        /// <param name="transactionHash">
        /// A hex-string representing the transaction hash.
        /// This value can be used e.g. on https://etherscan.io to lookup the transaction
        /// and see more information about it.
        /// </param>
        /// <param name="from">A hex-string denoting the sender of the transaction.</param>
        /// <param name="to">A hex-string denoting the recipient of the transaction.</param>
        /// <param name="feeInWei">A BigInteger representing the wei-denominated value which the sender paid for the transaction (i.e. gas multiplied with gas price).</param>
        /// <param name="amountInWei">A BigInteger representing the wei-denominated value which the sender sent to the recipient.</param>
        internal Transaction(
            Block block,
            string transactionHash,
            string from,
            string to,
            BigInteger feeInWei,
            BigInteger amountInWei)
        {
            this.Block = block;
            this.TxHash = transactionHash;
            this.From = from;
            this.To = to;
            this.AmountInWei = amountInWei;
            this.FeeInWei = feeInWei;
        }

        /// <summary>
        /// Gets the information about the block, in which this transaction was included.
        /// </summary>
        public Block Block { get; }

        /// <summary>
        /// Gets the transaction hash.
        /// </summary>
        public string TxHash { get; }

        /// <summary>
        /// Gets the address of the sender of this transaction (hex-string).
        /// </summary>
        public string From { get; }

        /// <summary>
        /// Gets the address of the recipient of this transaction (hex-string).
        /// </summary>
        public string To { get; }

        /// <summary>
        /// Gets the wei-denominated amount which was sent using this transaction (not including fees).
        /// </summary>
        public BigInteger AmountInWei { get; }

        /// <summary>
        /// Gets the wei-denominated amount which was paid in fees for this transaction.
        ///
        /// I.e. the transaction's spent gas value multiplied by the gas price value.
        /// </summary>
        public BigInteger FeeInWei { get; }

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