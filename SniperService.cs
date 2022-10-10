using BscTokenSniper.Handlers;
using BscTokenSniper.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nethereum.BlockchainProcessing;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace BscTokenSniper
{
    public class SniperService : IHostedService
    {
        private readonly Web3 _bscWeb3;
        private readonly Contract _factoryContract;
        private SniperConfiguration _sniperConfig;
        private bool _disposed;
        private StreamingWebSocketClient _client;
        private readonly List<IDisposable> _disposables = new();
        private readonly RugHandler _rugChecker;
        private readonly TradeHandler _tradeHandler;
        private readonly CancellationTokenSource _processingCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        public SniperService(IOptions<SniperConfiguration> options, RugHandler rugChecker, TradeHandler tradeHandler)
        {
            _sniperConfig = options.Value;            
            _bscWeb3 = new Web3(url: _sniperConfig.BscHttpApi, account: new Account(_sniperConfig.WalletPrivateKey));
            _factoryContract = _bscWeb3.Eth.GetContract(File.ReadAllText("./Abis/PairCreated.json"), _sniperConfig.PancakeswapFactoryAddress);
            _rugChecker = rugChecker;
            _tradeHandler = tradeHandler;
        }
        
        private void CreateTokenPair(FilterLog log, Action<EventLog<PairCreatedEvent>> onNext)
        {
            var pairCreated = log.DecodeEvent<PairCreatedEvent>();
            Log.Logger.Information("CreateTokenPair: Pair Created Event Found: {@log} Data: {@pairCreated}", log, pairCreated);
            if (onNext != null)
            {
                onNext.Invoke(pairCreated);
            }
        }

        private async Task<EthLogsObservableSubscription> StartClient()
        {
            _client = new StreamingWebSocketClient(_sniperConfig.BscNode);
            var filter = _bscWeb3.Eth.GetEvent<PairCreatedEvent>(_sniperConfig.PancakeswapFactoryAddress).CreateFilterInput();
            var filterTransfers = Event<PairCreatedEvent>.GetEventABI().CreateFilterInput();
            filterTransfers.Address = new string[1] { _sniperConfig.PancakeswapFactoryAddress };

            var subscription = new EthLogsObservableSubscription(_client);
            _disposables.Add(subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(t =>
            {
                try
                {
                    var decodedEvent = t.DecodeEvent<PairCreatedEvent>();
                    _ = PairCreated(decodedEvent).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, nameof(StartClient));
                }
            }));

            _addLiquidityBlockProcessor = _bscWeb3.Processing.Blocks.CreateBlockProcessor(steps =>
            {
                //match the to address and function signature
                steps.TransactionStep.SetMatchCriteria(t =>
                    t.Transaction.IsTo(_sniperConfig.PancakeswapRouterAddress) &&
                t.Transaction.IsTransactionForFunctionMessage<AddLiquidityETHFunction>());

                steps.TransactionStep.AddProcessorHandler(l => NewAddLiquidityEvent(l));
            });

            await _client.StartAsync();
            await subscription.SubscribeAsync(filter);
            return subscription;
        }

        private async Task NewAddLiquidityEvent(TransactionVO l)
        {
            var addLiquidityEvent = l.Transaction.DecodeTransactionToFunctionMessage<AddLiquidityETHFunction>();

            var getPairFunction= _factoryContract.GetFunction("getPair");
            var pairAddress = await getPairFunction.CallAsync<string>(_sniperConfig.LiquidityPairAddress, addLiquidityEvent.Token);
        
            Log.Logger.Information("NewAddLiquidityEvent:Add Liquidity Event detected: {@addLiquidityEvent}", addLiquidityEvent);
            var newPair = new PairCreatedEvent
            {
                Amount = addLiquidityEvent.AmountETHMin,
                Pair = pairAddress,
                Token0 = _sniperConfig.LiquidityPairAddress,
                Token1 = addLiquidityEvent.Token
            };

            await PairCreated(newPair);
        }

        private BigInteger _currentBlock = BigInteger.Zero;
        private BlockchainCrawlingProcessor _addLiquidityBlockProcessor;

        private void KeepAliveClient()
        {
            while (!_disposed)
            {
                var handler = new EthBlockNumberObservableHandler(_client);
                var disposable = handler.GetResponseAsObservable().Subscribe(t =>
                {
                    _currentBlock = t.Value;
                    Serilog.Log.Logger.Information("KeepAliveClient: Current Block: {0}", t);
                    _ = _addLiquidityBlockProcessor.ExecuteAsync(toBlockNumber: _currentBlock,
                        cancellationToken: CancellationToken.None,
                        startAtBlockNumberIfNotProcessed: _currentBlock).ConfigureAwait(false);
                });
                try
                {
                    handler.SendRequestAsync().Wait(TimeSpan.FromSeconds(10));
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error(e, nameof(KeepAliveClient));
                    Serilog.Log.Logger.Information("KeepAliveClient: Error from websocket, restarting client.");
                    _ = StartClient().Result;
                }
                Thread.Sleep(2500);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartClient();
            new Thread(KeepAliveClient).Start();
        }

        private async Task PairCreated(EventLog<PairCreatedEvent> pairCreated)
        {
            PairCreatedEvent pair = null;
            try
            {
                pair = pairCreated.Event;
                await PairCreated(pair);
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("PairCreated: {0}: PairCreated token {1} Error {2}", nameof(PairCreated), pair.Symbol, e.ToString());                                                       
            }
        }

        private async Task PairCreated(PairCreatedEvent pair)
        {
            var otherPairAddress = pair.Token0.Equals(_sniperConfig.LiquidityPairAddress, StringComparison.InvariantCultureIgnoreCase) ? pair.Token1 : pair.Token0;
        
            var symbol = await _rugChecker.GetSymbol(pair);
            pair.Symbol = symbol;

            Log.Logger.Warning("PairCreated event. Processing Symbol: {0} Address: {1} Token1: {2} Token0: {3}", symbol, otherPairAddress, pair.Token1, pair.Token0);

            var addressWhitelisted = _sniperConfig.WhitelistedTokens.Any(t => t.Equals(otherPairAddress));
            if(_sniperConfig.OnlyBuyWhitelist && !addressWhitelisted)
            {
                Log.Logger.Warning("PairCreated: Address is not in the whitelist blocked {0}", otherPairAddress);
                return;
            }        

            var rugCheckPassed = _sniperConfig.RugCheckEnabled && !addressWhitelisted ? await _rugChecker.CheckRugAsync(pair) : true;
            var otherTokenIdx = pair.Token0.Equals(_sniperConfig.LiquidityPairAddress, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
            var honeypotCheck = !addressWhitelisted && _sniperConfig.HoneypotCheck;

            Log.Logger.Information("PairCreated: Discovered Token Pair: {0} Rug check Result: {1} Contract address: {2}", symbol, rugCheckPassed, otherPairAddress);

            //AJB                        
            //TODO
            TokensOwned token = _tradeHandler.GetToken(otherPairAddress);
            if (token == null)
            {
                _tradeHandler.SaveTokens(otherPairAddress, otherTokenIdx, pair.Pair, symbol, "TokenAdded", "New Token/Pair added");
                Log.Logger.Information("PairCreated: Discovered new Token Pair: {0} Contract address: {1}", symbol, otherPairAddress);
                
            }
            else {
                Log.Logger.Information("PairCreated: Token Pair exists {0} Contract address: {1}", symbol, otherPairAddress);
            }


            if (!rugCheckPassed)
            {
                Log.Logger.Warning("PairCreated: Rug Check failed for {0}", symbol);

                //AJB
                _tradeHandler.SaveRejectedTokens(otherPairAddress, otherTokenIdx, pair.Pair, symbol, "RugCheck", "Rug check failed", honeypotCheck);
                return;
            }

            Log.Logger.Information("PairCreated: Starting Honeypot check for {0} with amount {1}", symbol, _sniperConfig.HoneypotCheckAmount);
            if (!honeypotCheck)
            {
                if (!addressWhitelisted)
                {
                    Log.Logger.Information("PairCreated: Buying Token pair: {0}", symbol);
                }
                else
                {
                    Log.Logger.Information("PairCreated: Buying Token pair: {0} WHITELISTED ADDRESS: {1}", symbol, addressWhitelisted);
                }
                
                await _tradeHandler.Buy(otherPairAddress, otherTokenIdx, pair.Pair, _sniperConfig.AmountToSnipe, symbol, "Buy", "Buying Token Pair: " + symbol);
                return;
            }

            var buySuccess = await _tradeHandler.Buy(otherPairAddress, otherTokenIdx, pair.Pair, _sniperConfig.HoneypotCheckAmount, symbol, "Buy", "Honeypot check", true);
            if (!buySuccess)
            {
                Log.Logger.Fatal("PairCreated: Honeypot check failed could not buy token: {0}", pair.Symbol);

                //AJB
                _tradeHandler.SaveRejectedTokens(otherPairAddress, otherTokenIdx, pair.Pair, symbol, "HoneypotCheckFailed", "Honeypot check failed for token " + pair.Symbol);

                return;
            }
            
            var ownedToken = _tradeHandler.GetOwnedTokens(otherPairAddress);
            await _tradeHandler.Approve(otherPairAddress);
            var marketPrice = await _tradeHandler.GetMarketPrice(ownedToken, ownedToken.Amount - 1);
            var sellSuccess = false;
            try
            {
                sellSuccess = await _tradeHandler.Sell(otherPairAddress, ownedToken.Amount - 1, marketPrice, _sniperConfig.SellSlippage, symbol);
            }
            catch (Exception e)
            {
                Serilog.Log.Error("PairCreated: Error Sell", e);
            }
            if (!sellSuccess)
            {
                Log.Logger.Fatal("PairCreated: Honeypot check DETECTED HONEYPOT could not sell token: {0}", pair.Symbol);
                return;
            }

            Log.Logger.Information("PairCreated: Honeypot check PASSED buying token: {0}", pair.Symbol);

            //Remove from Reject Token collection
            if (_tradeHandler.IsRejectedToken(otherPairAddress))
            {
                _tradeHandler.RemoveRejectedTokenFromList(otherPairAddress);
                Log.Logger.Warning("PairCreated: PairCreated event. We've removed this coin from the rejected list {0}", otherPairAddress);
                return;
            }         

            await _tradeHandler.Buy(otherPairAddress, otherTokenIdx, pair.Pair, _sniperConfig.AmountToSnipe, symbol, "HoneypotCheckPassed", "Honeypot check passed for token " + pair.Symbol);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information("StopAsync: Stopping SniperService");
            _disposed = true;
            _processingCancellation.Dispose();
            _disposables.ForEach(t => t.Dispose());
            return Task.CompletedTask;
        }
    }
}