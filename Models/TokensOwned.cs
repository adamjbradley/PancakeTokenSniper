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
    public class TokensOwned
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
    
    public static class MyTokensOwned
    {
        public static ConcurrentDictionary<String, TokensOwned> LoadTokensFromFile(string filename)     
        {            
            try {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ConcurrentDictionary<String, TokensOwned> tokensOwned = (ConcurrentDictionary<String, TokensOwned>)serializer.Deserialize(file, typeof(ConcurrentDictionary<String, TokensOwned>));                    
                    return tokensOwned;
                }
            } catch (Exception e) {
                Console.WriteLine("File does not exist " + filename);
                return new ConcurrentDictionary<String, TokensOwned>();
            }            
        }
        
        public static bool SaveTokens(ConcurrentDictionary<String, TokensOwned> tokens, String filename)
        {
            // serialize JSON to a string and then write string to a file
            File.WriteAllText(filename, JsonConvert.SerializeObject(tokens));
            return true;
        }

    }

}
