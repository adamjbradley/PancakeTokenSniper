using BscTokenSniper.Models;
using Fractions;
using Microsoft.Extensions.Options;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
namespace BscTokenSniper.Handlers
{
    public class TradeHandler : IDisposable
    {
        private readonly SniperConfiguration _sniperConfig;
        private readonly Web3 _bscWeb3;
        private readonly Contract _pancakeContract;
        private readonly RugHandler _rugChecker;
        private ConcurrentDictionary<String, TokensOwned> _ownedTokenList = new ConcurrentDictionary<String,TokensOwned>();
        private bool _stopped;
        private readonly string _erc20Abi;
        private readonly string _pairAbi;
        private static BigInteger Max { get; } = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935");
        private ConcurrentDictionary<String, TokensOwned> _rejectedTokenList = new ConcurrentDictionary<String,TokensOwned>();
        private ConcurrentDictionary<String, TokensOwned> _soldTokenList = new ConcurrentDictionary<String,TokensOwned>();
        private ConcurrentDictionary<String, TokensOwned> _goodTokenList = new ConcurrentDictionary<String,TokensOwned>();
        private ConcurrentDictionary<String, TokensOwned> _tokenList = new ConcurrentDictionary<String,TokensOwned>();

        public TradeHandler(IOptions<SniperConfiguration> options, RugHandler rugChecker)
        {
            _sniperConfig = options.Value;
            //_bscWeb3 = new Web3(url: _sniperConfig.BscHttpApi, account: new Account(_sniperConfig.WalletPrivateKey, new BigInteger(_sniperConfig.ChainId)));
            _bscWeb3 = new Web3(url: _sniperConfig.BscHttpApi, account: new Account(_sniperConfig.WalletPrivateKey, new BigInteger(_sniperConfig.ChainId)));
            _bscWeb3.TransactionManager.UseLegacyAsDefault = true;
            _erc20Abi = File.ReadAllText("./Abis/Erc20.json");
            _pairAbi = File.ReadAllText("./Abis/Pair.json");
            _pancakeContract = _bscWeb3.Eth.GetContract(File.ReadAllText("./Abis/Pancake.json"), _sniperConfig.PancakeswapRouterAddress);
            _rugChecker = rugChecker;

            _ownedTokenList = MyTokensOwned.LoadTokensFromFile("ownedtokens.json");
            _goodTokenList = MyTokensOwned.LoadTokensFromFile("goodtokens.json");
            _rejectedTokenList = MyTokensOwned.LoadTokensFromFile("rejectedtokens.json");
            _soldTokenList = MyTokensOwned.LoadTokensFromFile("soldtokens.json");
            _tokenList = MyTokensOwned.LoadTokensFromFile("tokens.json");
            Start();
        }

        public async Task<bool> Buy(string tokenAddress, int tokenIdx, string pairAddress, double amt, string symbol, string acceptReason, string detail, bool honeypotCheck = false)
        {
            try
            {
                if (_ownedTokenList.ContainsKey(tokenAddress)) {
                    Log.Logger.Information("Buy: [CANNOT BUY] Token: {0} Cause: {1}", tokenAddress, "Already has token");
                    return false;
                }
                else {
                    _goodTokenList.TryAdd(tokenAddress, new TokensOwned
                    {
                        Address = tokenAddress,
                        Amount = -1,
                        BnbAmount = -1,
                        SinglePrice = -1,
                        TokenIdx = tokenIdx,
                        PairAddress = pairAddress,
                        Decimals = -1,
                        HoneypotCheck = honeypotCheck,
                        Symbol = symbol,
                        Timestamp = DateTime.UtcNow,
                        AcceptReason = acceptReason,
                        Detail = detail
                    });

                    //AJB                    
                    MyTokensOwned.SaveTokens(_goodTokenList, "goodtokens.json");
                }

                if (_sniperConfig.BuyDelaySeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_sniperConfig.BuyDelaySeconds));
                }
                var buyFunction = _pancakeContract.GetFunction("swapExactETHForTokens");
                var gas = new HexBigInteger(_sniperConfig.GasAmount);
                var amount = new HexBigInteger(Web3.Convert.ToWei(amt));
                var buyReturnValue = await buyFunction.SendTransactionAsync(_sniperConfig.WalletAddress, gas, amount, 0,
                    new string[] { _sniperConfig.LiquidityPairAddress, tokenAddress },
                    _sniperConfig.WalletAddress,
                    (DateTime.UtcNow.Ticks + _sniperConfig.TransactionRevertTimeSeconds));
                var reciept = await _bscWeb3.TransactionManager.TransactionReceiptService.PollForReceiptAsync(buyReturnValue, new CancellationTokenSource(TimeSpan.FromMinutes(2)));
                var sellPrice = await GetMarketPrice(new TokensOwned
                {
                    PairAddress = pairAddress,
                    TokenIdx = tokenIdx
                }, amount);
                Log.Logger.Information("Buy: [BUY] TX ID: {buyReturnValue} Reciept: {@reciept}", buyReturnValue, reciept);

                var swapEventList = reciept.DecodeAllEvents<SwapEvent>().Where(t => t.Event != null)
                    .Select(t => t.Event).ToList();
                var swapEvent = swapEventList.FirstOrDefault();
                if (swapEvent != null)
                {
                    var balance = tokenIdx == 0 ? swapEvent.Amount0Out : swapEvent.Amount1Out;
                    var erc20Contract = _bscWeb3.Eth.GetContract(_erc20Abi, tokenAddress);
                    var decimals = await erc20Contract.GetFunction("decimals").CallAsync<int>();                    

                    _ownedTokenList.TryAdd(tokenAddress, new TokensOwned
                    {
                        Address = tokenAddress,
                        Amount = balance,
                        BnbAmount = amount,
                        SinglePrice = sellPrice,
                        TokenIdx = tokenIdx,
                        PairAddress = pairAddress,
                        Decimals = decimals,
                        HoneypotCheck = honeypotCheck,
                        Symbol = symbol,
                        Timestamp = DateTime.UtcNow,
                        AcceptReason = acceptReason,
                        Detail = detail
                    });

                    //AJB                    
                    MyTokensOwned.SaveTokens(_ownedTokenList, "ownedtokens.json");
                        
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Logger.Error("Buy: Error buying", e);
                return false;
            }
        }

        public TokensOwned GetToken(string tokenAddress)
        {
            TokensOwned value;
            if (_tokenList.TryGetValue(tokenAddress, out value))
                return value;
            else
                return null;
        }

        public TokensOwned GetOwnedTokens(string tokenAddress)
        {
            TokensOwned value;
            if (_ownedTokenList.TryGetValue(tokenAddress, out value))
                return value;
            else
                return null;
        }

        public bool RemoveOwnedTokenFromList(string tokenAddress)
        {
            TokensOwned value;
            if (_ownedTokenList.TryRemove(tokenAddress, out value))
                return true;
            else
                return false;
        }

        ///
        /// Rejected Tokens
        ///

        public bool IsRejectedToken(string tokenAddress)
        {
            TokensOwned value;
            if (_ownedTokenList.TryGetValue(tokenAddress, out value))
                return true;
            else
                return false;
        }

        public bool RemoveRejectedTokenFromList(string tokenAddress)
        {
            Log.Logger.Warning("TradeHandler: RemoveRejectedTokenFromList removing this coin from the rejected list {0}", tokenAddress);

            TokensOwned value;
            if (this._rejectedTokenList.TryRemove(tokenAddress, out value)) 
            {
                this.SaveRejectedTokens();
                return true;
            }
            else
                return false;
        }

        public bool SaveRejectedTokens()
        {            
            return MyTokensOwned.SaveTokens(this._rejectedTokenList, "rejectedtokens.json");        
        }

        public bool SaveRejectedTokens(string address, int tokenIdx, string pairAddress, string symbol, string rejectReason, string detail, bool honeypotCheck = false)
        {            
            if (!_rejectedTokenList.ContainsKey(address))
            {
                Log.Logger.Information("SaveRejectedTokens: Saving Token {0} to the _rejectedTokenList. Address: {1}  Pair Address: {2} ", symbol, address, pairAddress);

                //AJB
                this._rejectedTokenList.TryAdd(address, new TokensOwned
                {
                    Address = address,
                    Amount = 0,
                    BnbAmount = 0,
                    SinglePrice = 0,
                    TokenIdx = tokenIdx,
                    PairAddress = pairAddress,
                    Decimals = 0,                    
                    Symbol = symbol,
                    Timestamp = DateTime.UtcNow,
                    RejectReason = rejectReason,                    
                    Detail = detail,
                    HoneypotCheck = true
                });
            }
            else {
                Log.Logger.Error("SaveRejectedTokens: This Token is already in the _rejectedTokenList! " + address);
            }            

            return MyTokensOwned.SaveTokens(this._rejectedTokenList, "rejectedtokens.json");
        }

        public bool SaveTokens(string address, int tokenIdx, string pairAddress, string symbol, string reason, string detail, bool honeypotCheck = false)
        {            
            if (!_tokenList.ContainsKey(address))
            {
                Log.Logger.Information("SaveTokens: Saving Token {0} to the _tokenList. Address: {1}  Pair Address: {2} ", symbol, address, pairAddress);

                //AJB
                this._tokenList.TryAdd(address, new TokensOwned
                {
                    Address = address,
                    Amount = 0,
                    BnbAmount = 0,
                    SinglePrice = 0,
                    TokenIdx = tokenIdx,
                    PairAddress = pairAddress,
                    Decimals = 0,                    
                    Symbol = symbol,
                    Timestamp = DateTime.UtcNow,
                    RejectReason = reason,                    
                    Detail = detail,
                    HoneypotCheck = true
                });
            }
            else {
                Log.Logger.Error("SaveTokens: This Token is already in the _tokenList! " + address);
            }            

            return MyTokensOwned.SaveTokens(this._tokenList, "tokens.json");
        }

        
        public async Task<bool> Approve(string tokenAddress)
        {
            try
            {
                var gas = new HexBigInteger(_sniperConfig.GasAmount);
                var pairContract = _bscWeb3.Eth.GetContract(_pairAbi, tokenAddress);
                var approveFunction = pairContract.GetFunction<ApproveFunction>();
                var approve = await approveFunction.SendTransactionAndWaitForReceiptAsync(new ApproveFunction
                {
                    Spender = _sniperConfig.PancakeswapRouterAddress,
                    Value = Max
                }, _sniperConfig.WalletAddress, gas, new HexBigInteger(BigInteger.Zero));
            }
            catch (Exception e)
            {
                Log.Logger.Warning("Approve: Could not approve sell for {0}", tokenAddress);
            }
            return true;
        }

        public async Task<bool> Sell(string tokenAddress, BigInteger amount, BigInteger outAmount, double slippage, string symbol)
        {
            try
            {
                var sellFunction = _pancakeContract.GetFunction<SwapExactTokensForETHSupportingFeeOnTransferTokensFunction>();

                var gas = new HexBigInteger(_sniperConfig.GasAmount);
                var transactionAmount = new BigInteger((decimal)amount).ToHexBigInteger();

                var txId = await sellFunction.SendTransactionAsync(new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
                {
                    AmountOutMin = slippage == 0 ? outAmount : (new Fraction(outAmount).Subtract(new Fraction(slippage/100.0).Multiply(outAmount)).ToBigInteger()),
                    AmountIn = amount,
                    Path = new List<string>() { tokenAddress, _sniperConfig.LiquidityPairAddress },
                    To = _sniperConfig.WalletAddress,
                    Deadline = new BigInteger(DateTime.UtcNow.Ticks + _sniperConfig.TransactionRevertTimeSeconds)
                }, _sniperConfig.WalletAddress, gas, new HexBigInteger(BigInteger.Zero));
                var reciept = await _bscWeb3.TransactionManager.TransactionReceiptService.PollForReceiptAsync(txId, new CancellationTokenSource(TimeSpan.FromMinutes(2)));
                Log.Logger.Information("Sell: [SELL] TX ID: {txId} Reciept: {@reciept}", txId, reciept);

                var swapEventList = reciept.DecodeAllEvents<SwapEvent>().Where(t => t.Event != null)
                    .Select(t => t.Event).ToList();
                var swapEvent = swapEventList.FirstOrDefault();
                if (swapEvent != null)
                {
                    //TODO
                    TokensOwned item;
                    if (_ownedTokenList.TryRemove(tokenAddress, out item))
                        _soldTokenList.TryAdd(tokenAddress, item);         
                    
                    //AJB
                    MyTokensOwned.SaveTokens(_ownedTokenList, "ownedtokens.json");
                    MyTokensOwned.SaveTokens(_soldTokenList, "soldtokens");

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Logger.Error("Sell: Error selling", e);
                return false;
            }
        }

        private BigInteger GetMarketPrice(Reserves price, TokensOwned ownedToken, BigInteger amount)
        {
            if (price.Reserve0 == 0 || price.Reserve1 == 0)
            {
                return BigInteger.Zero;
            }
            var pricePerLiquidityToken = ownedToken.TokenIdx == 1 ? new Fraction(price.Reserve0).Divide(price.Reserve1) : new Fraction(price.Reserve1).Divide(price.Reserve0);

            return ((BigInteger)pricePerLiquidityToken.Multiply(amount));
        }

        public async Task<BigInteger> GetMarketPrice(TokensOwned ownedToken, BigInteger amount)
        {
            var price = await _rugChecker.GetReserves(ownedToken.PairAddress);
            return GetMarketPrice(price, ownedToken, amount);
        }

        public void Start()
        {
            new Thread(new ThreadStart(MonitorPrices)).Start();
        }

        private void MonitorPrices()
        {
            while (!_stopped)
            {
            
                var enumerator = _ownedTokenList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    TokensOwned ownedToken = enumerator.Current.Value;          

                    if(ownedToken.FailedSell || ownedToken.HoneypotCheck)
                    {
                        continue;
                    }
                    var price = _rugChecker.GetReserves(ownedToken.PairAddress).Result;
                    if(price.Reserve0 == 0 || price.Reserve1 == 0)
                    {
                        continue;
                    }

                    var currentPrice = GetMarketPrice(price, ownedToken, ownedToken.BnbAmount);
                    var profitPerc = new Fraction(currentPrice).Subtract(ownedToken.SinglePrice).Divide(ownedToken.SinglePrice).Multiply(100);
                    Log.Logger.Information("MonitorPrices: Token {0} Price bought: {1} Current Price: {2} Current Profit: {3}%",
                        ownedToken.Address, ((decimal)ownedToken.SinglePrice), ((decimal)currentPrice), ((decimal)profitPerc));

                    if (profitPerc > new Fraction(_sniperConfig.ProfitPercentageMargin))
                    {
                        try
                        {
                            ownedToken.FailedSell = !Sell(ownedToken.Address, ownedToken.Amount - 1, GetMarketPrice(ownedToken, ownedToken.Amount - 1).Result, _sniperConfig.SellSlippage, ownedToken.Symbol).Result;
                        } catch(Exception e)
                        {
                            Log.Logger.Error(nameof(MonitorPrices), e);
                            ownedToken.FailedSell = true;
                        }

                        TokensOwned removedToken;
                        _ownedTokenList.TryRemove(ownedToken.Address, out removedToken);                        
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));          
                }
            }
        }

        public void Dispose()
        {
            _stopped = true;
        }
    }
}
