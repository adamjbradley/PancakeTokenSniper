using System;
using System.Linq;
using Serilog;
using System.Numerics;

namespace BscTokenSniper.Handlers
{
    public class CoinAndLiquidityHandler
    {
        #region Liquidity Events

        //
        // Liquidity Event
        // 
        public bool AddLiquidityEvent(string amount, string pairAddress, string token0, string token1)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering AddLiquidityEvent");

            try
            {
                using (var db = new PersistenceContext())
                {
                    //Int64 longAmount = Convert.ToInt64(amount);
                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: AddTokenPair converting BigInt to String");
                    string longAmount = Convert.ToString(amount);
                    LiquidityEvent le = new LiquidityEvent(longAmount, pairAddress, token0, token1);

                    TokenPair tp = this.GetTokenPairs(pairAddress);
                    if (tp == null)
                    {
                        Serilog.Log.Logger.Error("CoinAndLiquidityHandler: AddLiquidityEvent Token Pair not found! {0}", pairAddress);
                        throw new Exception("CoinAndLiquidityHandler: AddLiquidityEvent Token Pair not found");
                    }

                    if (tp.LiquidityEvents != null)
                        tp.LiquidityEvents.Add(le);

                    db.Update(tp);
                    db.SaveChanges();

                    Serilog.Log.Logger.Information("AddLiquidityEvent: CoinAndLiquidityHandler successfully persisted event for {0} : {1}", pairAddress, amount);
                    return true;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, "CoinAndLiquidityHandler: " + nameof(AddLiquidityEvent));
                return false;
            }
        }

        public bool GetLiquidityEvent(string address)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering GetLiquidityEvent");
            using (var db = new PersistenceContext())
            {
                try
                {
                    var le = db.LiquidityEvents
                        .OrderBy(b => b.LiquidityEventId)
                        .First();

                    if (le != null)
                    {
                        Serilog.Log.Logger.Information("CoinAndLiquidityHandler: GetLiquidityEvent found liquidity event {0}", address);
                        return true;
                    }
                    else
                    {
                        Serilog.Log.Logger.Information("CoinAndLiquidityHandler: GetLiquidityEvent did not find this liquidity event {0}", address);
                    }
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, nameof(GetLiquidityEvent));
                    throw;
                }
                return false;
            }
        }

        public bool LiquidityEventExists(string address)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering LiquidityEventExists");
            if (GetLiquidityEvent(address))
                return true;
            else
                return false;
        }
        #endregion

        #region Token Pairs
        //
        // TokenPairs
        // 

        public TokenPair AddTokenPair(string address, string symbol, string token0, string token1)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering AddTokenPair");
            try
            {
                using (var db = new PersistenceContext())
                {
                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: AddTokenPair adding token pair {0} at address {1}", symbol, address);
                    TokenPair tp = new TokenPair(address, symbol, token0, token1);
                    db.Add(tp);
                    db.SaveChanges();
                    return tp;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                throw;
            }
        }
        #endregion        

        public bool TokenPairExists(string address)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering TokenPairExists");
            if (this.GetTokenPairs(address) != null)
                return true;
            else
                return false;
        }

        public TokenPair GetTokenPairs(string address)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering GetLiquidityEvent");
            try
            {
                using (var db = new PersistenceContext())
                {
                    var tp = db.TokenPairs
                        .Where(b => b.Address.Contains(address))
                        .ToList<TokenPair>();

                    if (tp != null)
                    {
                        if (tp.Count<TokenPair>() == 0)
                            return null;
                        else
                        {
                            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: GetTokenPairs found Token Pair {0}", address);
                            return tp.First<TokenPair>();
                        }
                    }
                    else
                    {
                        Serilog.Log.Logger.Information("CoinAndLiquidityHandler: GetTokenPairs did not find Token Pair {0}", address);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, nameof(GetLiquidityEvent));
                throw;
            }

        }

        public bool UpdateTokenPair(string address, TokenPair tokenPair)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering UpdateTokenPair");
            
            try
            {                
                using (var db = new PersistenceContext())
                {
                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: UpdateTokenPair adding token pair {0} at address {1}", tokenPair.Symbol, address);
                    
                    db.Update(tokenPair);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                throw;
            }

        }


    }
}