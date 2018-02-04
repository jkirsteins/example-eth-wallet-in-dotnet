using System;
using System.Numerics;
using Newtonsoft.Json;

namespace wallet {
    struct Transaction {
        public BigInteger BlockNumber;
        public DateTimeOffset BlockCreatedAt;
        public string TxHash;
        public string From;
        public string To;
        public BigInteger AmountInWei;
        public BigInteger FeeInWei;
        [JsonIgnore] public decimal AmountInEth => AmountInWei.WeiToEth();
        [JsonIgnore] public decimal FeeInEth => FeeInWei.WeiToEth();
    }
}