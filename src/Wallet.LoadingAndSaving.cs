// Copyright (c) 2018 Janis Kirsteins.
// Licenced under the MIT licence.
// See the LICENSE file in the project root for more information.

namespace DemoWallet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Threading.Tasks;
    using Nethereum.RPC.Eth.DTOs;
    using Nethereum.Signer;
    using Nethereum.Web3;
    using Nethereum.Web3.Accounts;
    using Newtonsoft.Json;
    using Serilog;

    /// <content>
    /// Contains methods for creating, saving, and loading the wallet to and from JSON
    /// files.
    /// </content>
    internal partial class Wallet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Wallet"/> class.
        ///
        /// This constructor is meant for the JSON deserializer (since the properties
        /// are immutable).
        /// </summary>
        /// <param name="lastProcessedBlock">Last processed block for this wallet (i.e. last block whose transactions we have examined).</param>
        /// <param name="knownTransactions">A list of known transactions.</param>
        /// <param name="privateKeyHexString">The private key (32 byte hex string) to use for this wallet.</param>
        [JsonConstructor]
        public Wallet(
            Block lastProcessedBlock,
            List<Transaction> knownTransactions,
            string privateKeyHexString)
        {
            this.LastProcessedBlock = lastProcessedBlock;
            this.KnownTransactions = knownTransactions;
            this.PrivateKeyHexString = privateKeyHexString;
        }

        private Wallet(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Loads a wallet from a JSON file at the given path.
        /// If the JSON file does not exist, a new wallet will be created,
        /// and associated with this path (associated so that subsequent
        /// calls to Save() will not need to provide the path).
        ///
        /// If the JSON file does not exist, it will be created, so
        /// ensure that you have write access.
        ///
        /// A newly created wallet will be initialized with the
        /// latest known block on the remode node as it's last
        /// processed block.
        /// </summary>
        /// <param name="file">Path to a wallet JSON file.</param>
        /// <param name="logger">A logger.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task will return a Wallet object associated with
        /// the given path.
        /// </returns>
        internal static async Task<Wallet> LoadOrCreate(string file, ILogger logger)
        {
            var result = FromJson(file, logger);
            if (result == null)
            {
                result = await Create(file, logger);
            }

            return result;
        }

        /// <summary>
        /// Invokes an Action which should change the state of this wallet, and
        /// saves the wallet's state to disk as a JSON file.
        /// It will use the path it was associated with during
        /// loading/creation.
        /// </summary>
        /// <param name="proc">
        /// An action which accepts this wallet as a parameter
        /// (for changing the state before saving).
        /// </param>
        internal void Save(Action<Wallet> proc)
        {
            this.Save(this.fileName, proc);
        }

        private static Wallet FromJson(string path, ILogger logger)
        {
            Wallet result = null;
            if (File.Exists(path))
            {
                var stateJson = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<Wallet>(stateJson);
                result.fileName = path;
                result.logger = logger;
            }

            return result;
        }

        private static async Task<Wallet> Create(string outputFile, ILogger logger)
        {
            var result = new Wallet(logger);
            result.Key = EthECKey.GenerateKey();

            var lastBlockNumber = await result.w3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

            /*  The Nethereum GetBlockWithTransactionsByNumber method sometimes
                returns null (maybe if the block contains no transactions?) and
                it does not appear to expose the eth_getBlockByNumber call.

                We could do the JSON RPC call manually, but in this case we just need
                the block for the timestamp.

                If we can not load the block for a brand-new wallet, we will just
                set the timestamp to the current time.
            */

            var optionalLastBlock = await result.w3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(lastBlockNumber);

            DateTimeOffset walletLastKnownStateTime;
            if (optionalLastBlock == null)
            {
                walletLastKnownStateTime = DateTimeOffset.Now;
            }
            else
            {
                walletLastKnownStateTime = DateTimeOffset.FromUnixTimeSeconds(
                    long.Parse(optionalLastBlock.Timestamp.Value.ToString()));
            }

            result.LastProcessedBlock = new Block(
                lastBlockNumber,
                walletLastKnownStateTime);

            result.Save(outputFile);
            return result;
        }

        private void Save(string path, Action<Wallet> proc)
        {
            proc(this);
            this.Save(path);
        }

        private void Save(string path)
        {
            this.fileName = path ?? throw new ArgumentNullException(nameof(path));
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, json);
        }
    }
}