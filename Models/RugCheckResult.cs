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
    public class RugCheckResult
    {
        public string Event { get; set; }
        public string EventResult { get; set; }
        public string Detail { get; set; }
        public string Symbol { get; set; }
        public string Address { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }

        public RugCheckResult(string _event, string eventResult, string detail, string symbol, string address, bool success)
        {
            Event = _event;
            EventResult = eventResult;
            Detail = detail;
            Symbol = symbol;
            Address = address;
            Success = success;                                
            Timestamp = DateTime.UtcNow;
        }
    }
}
