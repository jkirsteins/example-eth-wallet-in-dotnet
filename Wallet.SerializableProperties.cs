// Copyright (c) 2018 Janis Kirsteins.
// Licenced under the MIT licence.
// See the LICENSE file in the project root for more information.

namespace DemoWallet
{
    using System.Collections.Generic;
    using Nethereum.Signer;
    using Nethereum.Web3;
    using Nethereum.Web3.Accounts;
    using Newtonsoft.Json;

    /// <content>
    /// Contains all the properties of the wallet, which
    /// will be serialized to and from the local JSON file.
    /// </content>
    internal partial class Wallet
    {
        /// <summary>
        /// Gets the information about the last synchronized block.
        ///
        /// The information received from this wallet can be considered valid up
        /// to this block. If the actual blockchain is longer (and it typically will), then
        /// everything past this block is unknown by the wallet (and it needs to be synchronized).
        ///
        /// We need to store this information, so that when
        /// we synchronize with the Ethereum node, we can synchronize the delta between this block,
        /// and the last known block on the node.
        ///
        /// This also stores the block's timestamp, which is useful for displaying a human-readable
        /// value for the last-known time the wallet's balance and transaction list was verified.
        /// </summary>
        internal Block LastProcessedBlock { get; private set; }

        /// <summary>
        /// Gets a list of known transactions sent to/from this wallet.
        /// </summary>
        internal List<Transaction> KnownTransactions { get; private set; } = new List<Transaction>();

        /// <summary>
        /// Gets this wallet's Ethereum address.
        /// </summary>
        /// <returns>A 20-byte hex-string.</returns>
        internal string Address => this.Key.GetPublicAddress();

        /// <summary>
        /// Gets or sets the private key as a hex string.
        ///
        /// Setting this value will re-initialize the Key property.
        /// </summary>
        /// <returns>A 32 byte hex-string representing a valid SECP256k1 private key.</returns>
        internal string PrivateKeyHexString
        {
            get => this.Key.GetPrivateKey();
            set
            {
                this.Key = new Nethereum.Signer.EthECKey(value);
            }
        }
    }
}