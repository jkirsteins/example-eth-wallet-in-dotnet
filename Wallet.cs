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

namespace wallet {
    partial class Wallet {
        private Web3 W3;
        private EthECKey _key;
        [JsonIgnore] public EthECKey Key { 
            get => _key;
            private set {
                _key = value;
                W3 = new Web3(new Account(value), "https://rinkeby.infura.io:443");
            }
        }
        public async Task<BigInteger> GetNonceForNextTransaction() {
            return await W3.Eth.Transactions.GetTransactionCount.SendRequestAsync(
                Address, 
                BlockParameter.CreatePending());
        }
        public async Task<string> SendTransaction(
            string recipient, 
            BigInteger amountInWei, 
            BigInteger gasPriceInWei,
            BigInteger gasAllowance,
            BigInteger? nonceOverride
            ) 
        {
            var nonce = nonceOverride ?? (await GetNonceForNextTransaction());

            var offlineTransaction = Web3.OfflineTransactionSigner.SignTransaction(
                PrivateKeyHexString,
                recipient, 
                amountInWei,
                nonce, 
                gasPriceInWei,
                gasAllowance);

            if (!Web3.OfflineTransactionSigner.VerifyTransaction(offlineTransaction)) {
                throw new InvalidOperationException("Failed to create a valid offline transaction for some reason.");
            }

            return await W3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + offlineTransaction);
        }
        
        public async Task<BigInteger> FetchBalance()
        {
            return await W3.Eth.GetBalance.SendRequestAsync(Address, new BlockParameter(new HexBigInteger(LastProcessedBlock.Number)));
        }

        public async Task<BigInteger> GetMaxKnownRemoteBlock()
        {
            var result = await W3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return result.Value;
        }

        public async Task LoadAndSaveTransactionsForBlock(BigInteger blockNumber)
        {
            var block = await W3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
            if (block == null) { return; }

            var blockTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(block.Timestamp.Value.ToString()));
                
            Save(walletBeingUpdated => {
                foreach (var tx in block.Transactions) {
                    var forMe = string.Equals(tx.To, Address, StringComparison.OrdinalIgnoreCase);
                    var fromMe = string.Equals(tx.From, Address, StringComparison.OrdinalIgnoreCase);
                    
                    if (forMe || fromMe) {
                        
                        walletBeingUpdated.KnownTransactions.Add(new Transaction {
                            BlockNumber = tx.BlockNumber.Value,
                            BlockCreatedAt = blockTime,
                            TxHash = tx.TransactionHash,
                            From = tx.From,
                            To = tx.To,
                            FeeInWei = tx.Gas.Value * tx.GasPrice.Value,
                            AmountInWei = tx.Value
                        });
                    }
                }

                walletBeingUpdated.LastProcessedBlock = new Block {
                    Number = blockNumber,
                    Timestamp = blockTime
                };   
            });
        }
    }

}