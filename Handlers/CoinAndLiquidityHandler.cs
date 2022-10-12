using System;
using System.Linq;
using Serilog;
using System.Numerics;
using BscTokenSniper.Handlers;

using Microsoft.EntityFrameworkCore;

namespace BscTokenSniper.Handlers
{
    public class CoinAndLiquidityHandler
    {
        #region Liquidity Events

        //
        // Liquidity Event
        // 
        public static bool AddLiquidityEvent(string amount, string address, string token0, string token1)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering AddLiquidityEvent");

            using (var db = new PersistenceContext())
            {
                try
                {
                    TokenPair tokenPair = GetTokenPairs(address);

                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: AddTokenPair converting BigInt to String");

                    #region Manage BigInt
                    //Int64 longAmount = Convert.ToInt64(amount);
                    //string longAmount = Convert.ToString(amount);

                    /*
                    long longAmount;
                    if (!long.TryParse(amount, out longAmount)) {                                           
                        longAmount = -1;
                        Serilog.Log.Logger.Error("PersistenceContext: LiquidityEvent error converting BigInt(string) to long. Original amount {0}", amount);
                    }
                    */
                    #endregion

                    LiquidityEvent le = new LiquidityEvent(amount, address, token0, token1);
                    if (tokenPair == null)
                    {
                        Serilog.Log.Logger.Error("CoinAndLiquidityHandler: AddLiquidityEvent Token Pair not found! {0}", address);
                        throw new Exception("CoinAndLiquidityHandler: AddLiquidityEvent Token Pair not found");
                    }

                    if (tokenPair.LiquidityEvents == null)
                        Serilog.Log.Logger.Error("CoinAndLiquidityHandler: AddLiquidityEvent No Liquidity Events found! {0}", address);
                    else
                        Serilog.Log.Logger.Information("CoinAndLiquidityHandler: AddLiquidityEvent Token Pair has existing Liquidity Events {0}", address);

                    tokenPair.LiquidityEvents.Add(le);

                    db.Update(tokenPair);
                    db.SaveChanges();

                    Serilog.Log.Logger.Information("AddLiquidityEvent: CoinAndLiquidityHandler successfully persisted event for {0} : {1}", address, amount);
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        Serilog.Log.Logger.Information("AddLiquidityEvent: CoinAndLiquidityHandler DbUpdateconcurrencyException for type {0}", entry.Entity.GetType().ToString());

                        if (entry.Entity is LiquidityEvent)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                if (proposedValue != databaseValue)
                                    Serilog.Log.Logger.Information("AddLiquidityEvent: CoinAndLiquidityHandler DbUpdateconcurrencyException - Property {0} Proposed Value {1} Database Value {2}", property, proposedValue, databaseValue);

                                // TODO: decide which value should be written to database
                                // proposedValues[property] = <value to be saved>;
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else if (entry.Entity is TokenPair)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {

                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                if (proposedValue != databaseValue)
                                    Serilog.Log.Logger.Information("AddLiquidityEvent: CoinAndLiquidityHandler DbUpdateconcurrencyException - Property {0} Proposed Value {1} Database Value {2}", property, proposedValue, databaseValue);

                                // TODO: decide which value should be written to database
                                // proposedValues[property] = <value to be saved>;
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }

                        else
                        {
                            throw new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                        }
                    }
                    throw;
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error("CoinAndLiquidityHandler: Leaving AddLiquidityEvent");
                    Serilog.Log.Logger.Error(e, "CoinAndLiquidityHandler: " + nameof(AddLiquidityEvent));
                    throw;
                }
            }

            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving AddLiquidityEvent");

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
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error("CoinAndLiquidityHandler: Leaving GetLiquidityEvent");
                    Serilog.Log.Logger.Error(e, nameof(GetLiquidityEvent));
                    throw;
                }

                Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving GetLiquidityEvent");
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

        public static bool AddTokenPair(string address, string symbol, string token0, string token1)
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
                    return true;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("CoinAndLiquidityHandler: Leaving AddTokenPair");
                Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                throw;
            }

            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving AddTokenPair");
        }
        #endregion

        public static bool TokenPairExists(string address)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering TokenPairExists");
            if (GetTokenPairs(address) != null)
                return true;
            else
            {
                Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving TokenPairExists");
                return false;
            }
        }

        public static TokenPair GetTokenPairs(string address)
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
                Serilog.Log.Logger.Error("CoinAndLiquidityHandler: Leaving GetLiquidityEvent");
                Serilog.Log.Logger.Error(e, nameof(GetLiquidityEvent));
                throw;
            }

            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving GetLiquidityEvent");
        }

        public static bool AddTokenPairEvent(string address, TokenEvent tokenEvent)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering UpdateTokenPair");

            try
            {
                using (var db = new PersistenceContext())
                {
                    TokenPair tokenPair = GetTokenPairs(address);
                    tokenPair.TokenPairEvents.Add(tokenEvent);
                    db.Update<TokenPair>(tokenPair);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("CoinAndLiquidityHandler: Leaving UpdateTokenPair");
                Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                throw;
            }
            finally
            {
                Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving UpdateTokenPair");
            }
        }

        public static bool UpdateTokenPair(string address, TokenPair tp)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering UpdateTokenPair");

            try
            {
                using (var db = new PersistenceContext())
                {
                    TokenPair tokenPair = GetTokenPairs(address);
                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: UpdateTokenPair adding token pair {0} at address {1}", tokenPair.Symbol, address);

                    db.Update<TokenPair>(tokenPair);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    Serilog.Log.Logger.Information("UpdateTokenPair: CoinAndLiquidityHandler DbUpdateconcurrencyException for type {0}", entry.Entity.GetType().ToString());

                    if (entry.Entity is TokenPair)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();

                        foreach (var property in proposedValues.Properties)
                        {
                            var proposedValue = proposedValues[property];
                            var databaseValue = databaseValues[property];

                            if (proposedValue != databaseValue)
                                Serilog.Log.Logger.Information("UpdateTokenPair: CoinAndLiquidityHandler DbUpdateconcurrencyException - Property {0} Proposed Value {1} Database Value {2}", property, proposedValue, databaseValue);

                            // TODO: decide which value should be written to database
                            // proposedValues[property] = <value to be saved>;
                        }

                        // Refresh original values to bypass next concurrency check
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Don't know how to handle concurrency conflicts for "
                            + entry.Metadata.Name);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("CoinAndLiquidityHandler: Leaving UpdateTokenPair");
                Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                throw;
            }

            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving UpdateTokenPair");

        }
        public static bool UpdateTokenPair(string address, LiquidityEvent liquidityEvent)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering UpdateTokenPair - Liquidity Event");

            using (var db = new PersistenceContext())
            {
                try
                {
                    TokenPair tokenPair = GetTokenPairs(address);
                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: UpdateTokenPair adding token pair {0} at address {1}", tokenPair.Symbol, address);

                    db.Update<TokenPair>(tokenPair);
                    db.SaveChanges();
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        Serilog.Log.Logger.Information("UpdateTokenPair: UpdateTokenPair DbUpdateconcurrencyException for type {0}", entry.Entity.GetType().ToString());

                        if (entry.Entity is TokenPair)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                if (proposedValue != databaseValue)
                                    Serilog.Log.Logger.Information("UpdateTokenPair: CoinAndLiquidityHandler DbUpdateconcurrencyException - Property {0} Proposed Value {1} Database Value {2}", property, proposedValue, databaseValue);

                                // TODO: decide which value should be written to database
                                // proposedValues[property] = <value to be saved>;
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                        }
                    }
                    throw;
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                    throw;
                }
            }

            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving UpdateTokenPair - Liquidity Event");
        }
        public static bool UpdateTokenPair(string address, TokenEvent tokenEvent)
        {
            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Entering UpdateTokenPair - TokenEvent");

            try
            {
                using (var db = new PersistenceContext())
                {
                    TokenPair tokenPair = GetTokenPairs(address);
                    Serilog.Log.Logger.Information("CoinAndLiquidityHandler: UpdateTokenPair adding token pair {0} at address {1}", tokenPair.Symbol, tokenPair.Address);

                    db.Update<TokenPair>(tokenPair);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    Serilog.Log.Logger.Information("UpdateTokenPair: CoinAndLiquidityHandler DbUpdateconcurrencyException for type {0}", entry.Entity.GetType().ToString());

                    if (entry.Entity is TokenPair)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();

                        foreach (var property in proposedValues.Properties)
                        {
                            var proposedValue = proposedValues[property];
                            var databaseValue = databaseValues[property];

                            if (proposedValue != databaseValue)
                                Serilog.Log.Logger.Information("UpdateTokenPair: CoinAndLiquidityHandler DbUpdateconcurrencyException - Property {0} Proposed Value {1} Database Value {2}", property, proposedValue, databaseValue);

                            // TODO: decide which value should be written to database
                            // proposedValues[property] = <value to be saved>;
                        }

                        // Refresh original values to bypass next concurrency check
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Don't know how to handle concurrency conflicts for "
                            + entry.Metadata.Name);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, nameof(AddTokenPair));
                throw;
            }

            Serilog.Log.Logger.Information("CoinAndLiquidityHandler: Leaving UpdateTokenPair - TokenEvent");

        }
    }
}