using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System;

namespace BscTokenSniper.Handlers
{
    public class PersistenceContext : DbContext
    {        
        public DbSet<LiquidityEvent> LiquidityEvents { get; set; }
        public DbSet<TokenPair> TokenPairs { get; set; }
        public DbSet<TokenPairValue> TokenPairValues { get; set; }
        public DbSet<TokenEvent> TokenEvents { get; set; }

        public string _connectionString {get; set; }

        public PersistenceContext()
        {        
        }

        public PersistenceContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=192.168.1.120;Database=iiprod_db;Username=postgres;Password=1password2");
    }

    [Index(nameof(Address))]
    public class TokenPair
    {
        public int TokenPairId { get; set; }
        public DateTime Timestamp { get; set;}

        public string Address { get; set; }
        public string Symbol { get; set; }
        public string Token0 {get; set; }
        public string Token1 {get; set; }
        public bool Owned { get; set; }
        public bool ToTrade { get; set; }
        public string State { get; set; }        

        public TokenPair(string address, string symbol, string token0, string token1) {
            this.Address = address;
            this.Symbol = symbol;
            this.Token0 = token0;
            this.Token1 = token1;
            this.Timestamp = DateTime.UtcNow;
        }        

        public List<LiquidityEvent> LiquidityEvents { get; } = new();
        public List<TokenEvent> TokenPairEvents { get; } = new();        
    }

    public class LiquidityEvent
    {
        public int LiquidityEventId { get; set; }

        public DateTime Timestamp { get; set;}
        
        public string Amount {get; set;}
        public string PairAddress {get; set;}

        public string Token0 {get; set;}

        public string Token1 {get; set;}

        public LiquidityEvent(string amount, string pairAddress, string token0, string token1) {
            this.Amount = amount;
            this.PairAddress = pairAddress;
            this.Token0 = token0;
            this.Token1 = token1;
            this.Timestamp = DateTime.UtcNow;
        }

        public int TokenPairId { get; set; }
        public TokenPair TokenPair { get; set; }
    }
    public class TokenPairValue
    {        
        public int TokenPairValueId { get; set; }

        public DateTime Timestamp { get; set;}
        public int Value { get; set; }
        
        public int TokenPairId { get; set; }
        public TokenPair TokenPair { get; set; }
    }

    [Index(nameof(Address))]
    public class TokenEvent
    {
        public int TokenEventId { get; set; }
        public DateTime Timestamp { get; set;}

        public string Address { get; set; }
        public string Description {get; set; }

        public string EventType {get; set; }        

        public long BuyValue { get; set; }

        public long BuyQuantity { get; set; }

        public string Wallet { get; set; }        
        public string WalletAddress { get; set; }        

        public TokenEvent(string address, string eventType, string description) {
            this.Address = address;   
            this.EventType = eventType;
            this.Description = description;            
            this.Timestamp = DateTime.UtcNow;
        }

        public TokenEvent(string address, string eventType, string description, long buyValue, long buyQuantity, string wallet = "default", string walletAddress = "default") {
            this.Address = address;   
            this.EventType = eventType;
            this.Description = description;  
            this.BuyValue = buyValue;
            this.BuyQuantity = buyQuantity;
            this.Wallet = wallet;
            this.WalletAddress = walletAddress;
            this.Timestamp = DateTime.UtcNow;
            
        }
    
        public int TokenPairId { get; set; }
        public TokenPair TokenPair { get; set; }
    }
}