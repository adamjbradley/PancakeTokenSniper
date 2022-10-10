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
    public class SmartContract
    {
        public string Body { get; set; }
        public string Address { get; set; }
        public string Symbol { get; set; }
        public string Hash { get; set; }
        public string Status { get; set; }
        public bool Ok { get; set; }
        public string File { get; set;}
        public DateTime Timestamp { get; set; }

        public SmartContract(string body, string address, string symbol, string status, bool ok)
        {
            Body = body;
            Address = address;
            Symbol = symbol;
            Status = status;
            Ok = ok;
            Hash = SmartContractOperations.HashString(body);
            Timestamp = DateTime.UtcNow;
        }
    }

    public static class SmartContractOperations
    {
        public static ConcurrentBag<SmartContract> LoadContractsFromFile(string filename)     
        {            
            try {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ConcurrentBag<SmartContract> contracts = (ConcurrentBag<SmartContract>)serializer.Deserialize(file, typeof(ConcurrentBag<SmartContract>));                    
                    return contracts;
                }
            } catch (Exception e) {
                Console.WriteLine("No Contracts file cache or file does not exist");
                return new ConcurrentBag<SmartContract>();
            }            
        }
        
        public static bool SaveContracts(ConcurrentBag<SmartContract> contracts)
        {
            if (!contracts.IsEmpty)
            {                
                SmartContract contract = contracts.Last<SmartContract>();            
                string filepath = null;
                if (contract.Ok)                
                    filepath =  "Contracts/ok/" + contract.Address + ".sol";
                else
                    filepath =  "Contracts/notok/" + contract.Address + ".sol";

                SaveIndividualContract(contract, filepath);

                // serialize JSON to a string and then write string to a file
                contracts.Last<SmartContract>().File = filepath;
                contracts.Last<SmartContract>().Body = null;
                File.WriteAllText("smartcontracts.json", JsonConvert.SerializeObject(contracts));
            }
            return true;
        }

        public static bool SaveIndividualContract(SmartContract contract, string filename)
        {
            File.WriteAllText(filename, contract.Body);
            return true;
        }

        public static string HashString(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }
            
            // Uses SHA256 to create the hash
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                // Convert the string to a byte array first, to be processed
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hashBytes = sha.ComputeHash(textBytes);
                
                // Convert back to a string, removing the '-' that BitConverter adds
                string hash = BitConverter
                    .ToString(hashBytes)
                    .Replace("-", String.Empty);

                return hash;
            }
        }
    }

}
