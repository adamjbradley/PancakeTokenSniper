using Fractions;
using Nethereum.Util;
using System.Numerics;

using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Newtonsoft.Json;

namespace BscTokenSniper.Models
{    
    public class Token
    {
        public string Address { get; set; }
        public BigInteger Amount { get; set; }
        public BigInteger BnbAmount { get; set; }
        public Fraction SinglePrice { get; set; }
        public int TokenIdx { get; set; }
        public string PairAddress { get; set; }
        public int Decimals { get; set; }
        public bool HoneypotCheck { get; set; }
        public bool FailedSell { get; set; }
        public string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public string AcceptReason {get; set;}
        public string RejectReason {get; set;}
        public string Detail {get; set;}
    }
}
