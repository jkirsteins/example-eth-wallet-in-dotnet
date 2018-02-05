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
    using Nethereum.Hex.HexTypes;
    using Nethereum.RPC.Eth.DTOs;
    using Nethereum.Signer;
    using Nethereum.Web3;
    using Nethereum.Web3.Accounts;
    using Newtonsoft.Json;
    using Serilog;

    /// <summary>
    /// This class represents an Ethereum wallet. It has an associated
    /// secp256k1 key, can be serialized to/from JSON files, synchronize with a remote
    /// node and send transactions.
    /// </summary>
    /// <content>
    /// Contains code for interacting with the Ethereum blockchain (Rinkeby).
    /// </content>
    internal partial class Wallet
    {
        private ILogger logger;
        private Web3 w3;
        private EthECKey key;
        private string fileName = null;

        private EthECKey Key
        {
            get => this.key;
            set
            {
                this.key = value;
                this.w3 = new Web3(new Account(value), "https://rinkeby.infura.io:443");
            }
        }

        /// <summary>
        /// Sends a transaction.
        /// </summary>
        /// <param name="recipient">A hex string representing the recipient's address.</param>
        /// <param name="amountInWei">Wei-denominated amount to send to the recipient.</param>
        /// <param name="gasPriceInWei">Wei-denominated price per each gas unit.</param>
        /// <param name="gasAllowance">Maximum number of gas units per transaction (typically 21000 for simple value transfers).</param>
        /// <param name="nonceOverride">
        /// Optional nonce value to use. If this value is null, then the nonce will be
        /// calculated using the function GetNonceForNextTransaction().</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task will return the successfully created transaction's hash. This hash
        /// can be used to lookup information about the transaction later, and to determine
        /// if it succeeded or failed, and how much it actually paid in fees.
        /// </returns>
        internal async Task<string> SendTransaction(
            string recipient,
            BigInteger amountInWei,
            BigInteger gasPriceInWei,
            BigInteger gasAllowance,
            BigInteger? nonceOverride)
        {
            var nonce = nonceOverride ?? (await this.GetNonceForNextTransaction());

            var offlineTransaction = Web3.OfflineTransactionSigner.SignTransaction(
                this.PrivateKeyHexString,
                recipient,
                amountInWei,
                nonce,
                gasPriceInWei,
                gasAllowance);

            if (!Web3.OfflineTransactionSigner.VerifyTransaction(offlineTransaction))
            {
                throw new InvalidOperationException("Failed to create a valid offline transaction for some reason.");
            }

            return await this.w3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + offlineTransaction);
        }

        /// <summary>
        /// Returns a nonce value which can be used for the next transaction.
        ///
        /// The nonce is determined by asking the remote node for the transaction
        /// count for the wallet's address (including pending transactions).
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task will return a BigInteger number.
        /// </returns>
        internal async Task<BigInteger> GetNonceForNextTransaction()
        {
            return await this.w3.Eth.Transactions.GetTransactionCount.SendRequestAsync(
                this.Address,
                BlockParameter.CreatePending());
        }

        /// <summary>
        /// Fetches the wei-denominated balance of the wallet from the remote node,
        /// as of the last processed block.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task will return a BigInteger representing a wei-denominated value.
        /// </returns>
        internal async Task<BigInteger> FetchBalanceAsOfLastProcessedBlock()
        {
            return await this.w3.Eth.GetBalance.SendRequestAsync(
                this.Address,
                new BlockParameter(new HexBigInteger(this.LastProcessedBlock.Number)));
        }

        /// <summary>
        /// The task will return the last known block number from the
        /// remote node.
        ///
        /// This corresponds to the eth_blockNumber RPC call.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task will return a BigInteger representing the block number.
        /// </returns>
        internal async Task<BigInteger> GetMaxKnownRemoteBlock()
        {
            var result = await this.w3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return result.Value;
        }

        /// <summary>
        /// Fetches information about every transaction for a given block
        /// from the remote node, and compares the to/from fields with the
        /// wallet's address.
        ///
        /// If any of the fields match, transaction information (block number,
        /// timestamp, transaction hash, from/to addresses, fee and amount values)
        /// is stored locally.
        /// </summary>
        /// <param name="blockNumber">Block number for which the transactions will be verified.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task LoadAndSaveTransactionsForBlock(BigInteger blockNumber)
        {
            var block = await this.w3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
            if (block == null)
            {
                return;
            }

            var blockTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(block.Timestamp.Value.ToString()));

            this.Save(walletBeingUpdated =>
            {
                foreach (var tx in block.Transactions)
                {
                    var forMe = string.Equals(tx.To, this.Address, StringComparison.OrdinalIgnoreCase);
                    var fromMe = string.Equals(tx.From, this.Address, StringComparison.OrdinalIgnoreCase);

                    this.logger.Debug("Verifying transaction {transactionHash}", tx.TransactionHash);

                    if (forMe || fromMe)
                    {
                        if (forMe)
                        {
                            this.logger.Information("Found incoming transaction with hash {transactionHash}", tx.TransactionHash);
                        }
                        else
                        {
                            this.logger.Information("Found outgoing transaction with hash {transactionHash}", tx.TransactionHash);
                        }

                        /*
                        In case we are repeating a scan for some reason (e.g. debugging),
                        we do not want to add transactions we already have found before.
                        */
                        var knownTransactions = walletBeingUpdated.KnownTransactions.FindAll(
                            o => string.Equals(o.TxHash, tx.TransactionHash, StringComparison.OrdinalIgnoreCase));
                        if (knownTransactions.Count > 0)
                        {
                            this.logger.Information("Not saving transaction {transactionHash} locally because it is already known.", tx.TransactionHash);
                            continue;
                        }

                        walletBeingUpdated.KnownTransactions.Add(new Transaction(
                            new Block(
                                tx.BlockNumber.Value,
                                blockTime),
                            tx.TransactionHash,
                            tx.From,
                            tx.To,
                            tx.Gas.Value * tx.GasPrice.Value,
                            tx.Value));
                    }
                }

                walletBeingUpdated.LastProcessedBlock = new Block(
                    blockNumber,
                    blockTime);
            });
        }
    }
}