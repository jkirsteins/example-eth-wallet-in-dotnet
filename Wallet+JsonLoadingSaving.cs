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

namespace wallet {
    struct Block {
        public BigInteger Number;
        public DateTimeOffset Timestamp;
    }
    partial class Wallet {
        private static Wallet FromJson(string path) {
            Wallet result = null;
            if (File.Exists(path)) {
                var stateJson = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<Wallet>(stateJson);
                result.FileName = path;
            } 
            return result;
        }

        private static async Task<Wallet> Create(string outputFile) {
            var result = new Wallet();
            result.Key = EthECKey.GenerateKey();
            
            var lastBlockNumber = await result.W3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var lastBlock = await result.W3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(lastBlockNumber);
            
            result.LastProcessedBlock = new Block {
                Number = lastBlock.Number,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(lastBlock.Timestamp.Value.ToString()))
            };

            result.Save(outputFile);
            return result;
        }

        public static async Task<Wallet> LoadOrCreate(string file) {
            var result = FromJson(file);
            if (result == null) {
                result = await Create(file);
            }
            return result;
        }

        
        
        public Block LastProcessedBlock;
        public List<Transaction> KnownTransactions = new List<Transaction>();
        [JsonIgnore] public string FileName = null;
        public string Address => Key.GetPublicAddress();
        // This will be set when loading from Json, so set the key automatically
        public string PrivateKeyHexString { 
            get => Key.GetPrivateKey();
            set {
                Key = new EthECKey(value);
            }
        }

        public void Save(string path) {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, json);
        }

        public void Save(Action<Wallet> proc) {
            this.Save(FileName, proc);
        }

        public void Save(string path, Action<Wallet> proc) {
            FileName = path ?? throw new ArgumentNullException(nameof(path));
            proc(this);
            this.Save(path);
        }
    }

}