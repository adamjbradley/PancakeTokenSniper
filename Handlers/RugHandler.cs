using BscTokenSniper.Models;
using Fractions;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace BscTokenSniper.Handlers
{
    public class RugHandler
    {
        private readonly string GetSourceUrl = "https://api.bscscan.com/api?module=contract&action=getsourcecode&address={0}&apikey={1}";
        private readonly string RugdocCheckUrl = "https://honeypot.api.rugdoc.io/api/honeypotStatus.js?address={0}&chain=bsc";
        private HttpClient _httpClient;
        private SniperConfiguration _sniperConfig;
        private readonly string _erc20Abi;
        private readonly Web3 _bscWeb3;
        private readonly string _pairContractStr;
        private ConcurrentBag<SmartContract> _smartContractList = new ConcurrentBag<SmartContract>();
        private readonly string SafuCheckUrl = "https://app.staysafu.org/api/scan?tokenAddress={0}&key={1}&holdersAnalysis={2}&chain=bsc";

        private CoinAndLiquidityHandler _coinAndLiquidityHandler = new CoinAndLiquidityHandler();
        //private List<RugCheckResult> results = new List<RugCheckResult>();

        public RugHandler(IOptions<SniperConfiguration> sniperConfig, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _sniperConfig = sniperConfig.Value;
            _erc20Abi = File.ReadAllText("./Abis/Erc20.json");            
            _bscWeb3 = new Web3(url: _sniperConfig.BscHttpApi, account: new Account(_sniperConfig.WalletPrivateKey));
            _pairContractStr = File.ReadAllText("./Abis/Pair.json");

            _smartContractList = SmartContractOperations.LoadContractsFromFile("smartcontracts.json");
        }

        public async Task<bool> RugdocCheck(string otherPairAddress)
        {

            //TokenPair tp = CoinAndLiquidityHandler.GetTokenPairs(otherPairAddress);

            if(!_sniperConfig.RugdocCheckEnabled)
            {                                
                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK", "NOT_ENABLED", "Rugdoc not enabled", true, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK", "NOT_ENABLED", "Rugdoc not enabled", true, null));

                //return new RugCheckResult("RUGDOC_CHECK", "NOT_ENABLED", "Not enabled", null, otherPairAddress, true);
                //results.Add(new RugCheckResult("RUGDOC_CHECK", "NOT_ENABLED", "Not enabled", null, otherPairAddress, true));
                return true;
            }

            var result = await _httpClient.GetAsync(string.Format(GetSourceUrl, otherPairAddress, _sniperConfig.BscScanApikey));
            var jObject = JObject.Parse(await result.Content.ReadAsStringAsync());
            var innerResult = jObject["result"][0];
            var srcCode = innerResult.Value<string>("SourceCode");

            var sc = new SmartContract(srcCode, otherPairAddress, null, null, false);
            string fullRugdocStr = null;

            try
            {
                var response = await _httpClient.GetAsync(string.Format(RugdocCheckUrl, otherPairAddress));
                var rugdocStr = await response.Content.ReadAsStringAsync();                
                fullRugdocStr = rugdocStr;
                var responseObject = JObject.Parse(rugdocStr);
                var valid = responseObject["status"].Value<string>().Equals("OK", StringComparison.InvariantCultureIgnoreCase);
                Serilog.Log.Logger.Information("RugdocCheck: Rugdoc check token {0} Status: {1} RugDoc Response: {2}", otherPairAddress, valid, rugdocStr);

                if (valid) {
                    sc.Ok = true;
                    sc.Status = "Bsc contract is ok! RugdocCheck - " + fullRugdocStr;
                }
                else {
                    sc.Ok = false;
                    sc.Status = "Bsc contract is not ok! Failed RugdocCheck - " + fullRugdocStr;
                }                
                _smartContractList.Add(sc);
                SmartContractOperations.SaveContracts(_smartContractList);        
                
                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CONTRACT_STATUS", sc.Status, null, sc.Ok, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CONTRACT_STATUS", sc.Status, null, sc.Ok, null));

                //results.Add(new RugCheckResult("RUGDOC_CHECK_CONTRACT_STATUS", null, sc.Status, null, otherPairAddress, sc.Ok));
                //return new RugCheckResult("RUGDOC_CHECK_CONTRACT_STATUS", null, sc.Status, null, otherPairAddress, sc.Ok);
                return valid;
            }
            catch (Exception e)
            {
                Serilog.Log.Error(nameof(RugdocCheck), e);

                sc.Ok = false;
                sc.Status = "Bsc contract is not ok! Failed RugdocCheck - " + fullRugdocStr;
                _smartContractList.Add(sc);
                SmartContractOperations.SaveContracts(_smartContractList);        
                
                //results.Add(new RugCheckResult("RUGDOC_CHECK", "", sc.Status, null, otherPairAddress, sc.Ok));
                //return new RugCheckResult("RUGDOC_CHECK", "", sc.Status, null, otherPairAddress, sc.Ok);

                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK", "BSC_CONTRACT_FAILED", sc.Status, sc.Ok, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK", "BSC_CONTRACT_FAILED", sc.Status, sc.Ok, null));
                return false;                            
            }
        }

        public async Task<bool> SafuCheck(string otherPairAddress)
        {
            
            if(!_sniperConfig.SafuCheckEnabled)
            {
                //results.Add(new RugCheckResult("SAFU_CHECK", "NOT_ENABLED", "Not enabled", null, otherPairAddress, true));
                //return new RugCheckResult("SAFU_CHECK", "NOT_ENABLED", "Not enabled", null, otherPairAddress, true);
                return true;
            }
            
            var result = await _httpClient.GetAsync(string.Format(GetSourceUrl, otherPairAddress, _sniperConfig.BscScanApikey));
            var jObject = JObject.Parse(await result.Content.ReadAsStringAsync());
            var innerResult = jObject["result"][0];
            var srcCode = innerResult.Value<string>("SourceCode");

            var sc = new SmartContract(srcCode, otherPairAddress, null, null, false);
            string fullSafuStr = null;

            try
            {
                var response = await _httpClient.GetAsync(string.Format(SafuCheckUrl, otherPairAddress, _sniperConfig.SafuApikey, true));
                var safuStr = await response.Content.ReadAsStringAsync();
                fullSafuStr = safuStr;
                var responseObject = JObject.Parse(safuStr);
                
                //var valid = responseObject["status"].Value<string>().Equals("OK", StringComparison.InvariantCultureIgnoreCase);
                //var valid = responseObject["result"]["totalScore"].Value<string>().Equals("OK", StringComparison.InvariantCultureIgnoreCase);

                //AJB
                //TODO
                var valid = true;

                if (fullSafuStr.Equals("{\"result\":{\"isToken\":false}}"))
                {                
                    valid = false;
                }
                
                try {
                    if (responseObject["result"]["trade"]["error"].Value<string>().Contains("Error:"))
                    {
                        valid = false;
                    }
                }
                catch (Exception e) {}

                try {
                    if (responseObject["result"]["trade"]["error"].Value<bool>().Equals(true))
                    {
                        valid = false;
                    }
                }
                catch (Exception e) {}

                if (responseObject["result"]["trade"]["isHoneypot"].Value<bool>().Equals(true)) 
                {
                    valid = false;
                }

                //TODO Should we use SAFU for Liquidity Checks? 
                //AJB 
                //try {
                //    if (responseObject["result"]["liquidity"]["status"].Value<string>().Contains("error"))
                //    {
                //        valid = false;
                //    }
                //}
                //catch (Exception e) {}

                if (float.Parse(responseObject["result"]["totalScore"].Value<string>()) < _sniperConfig.SafuTotalScore)
                {
                    valid = false;                    
                }

                if (valid) {
                    sc.Ok = true;
                    sc.Status = "Bsc contract is ok! SafuCheck - " + fullSafuStr;
                }
                else {
                    sc.Ok = false;
                    sc.Status = "Bsc contract is not ok! Failed SafuCheck - " + fullSafuStr;
                }                
                _smartContractList.Add(sc);
                //_smartContractList.Append<SmartContract>(sc);
                SmartContractOperations.SaveContracts(_smartContractList);

                Serilog.Log.Logger.Information("SafuCheck: SafuCheck token {0} Status: {1} Response: {2}", otherPairAddress, valid, fullSafuStr);

                //results.Add(new RugCheckResult("SAFU_CHECK", "", sc.Status, null, otherPairAddress, sc.Ok));
                return valid;
                //return new RugCheckResult("SAFU_CHECK", "", sc.Status, null, otherPairAddress, sc.Ok);
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("SafuCheck: {0}: SafuCheck token {1} Response: {2} Error {3}", nameof(SafuCheck), otherPairAddress, fullSafuStr, e.ToString());                                        
                sc.Ok = false;
                sc.Status = "Bsc contract is not ok! Failed SafuCheck - " + nameof(SafuCheck) + ": SafuCheck Token: " +  otherPairAddress + " Response: " + fullSafuStr + " Error: " + e.ToString();
                
                _smartContractList.Add(sc);                
                SmartContractOperations.SaveContracts(_smartContractList);        

                //results.Add(new RugCheckResult("SAFU_CHECK", "EXCEPTION", e.ToString(), null, otherPairAddress, false));
                return false;
                //return new RugCheckResult("SAFU_CHECK", "EXCEPTION", e.ToString(), null, otherPairAddress, false);
            }
        }

        public async Task<string> GetSymbol(PairCreatedEvent pairCreatedEvent)
        {
            var otherPairAddress = pairCreatedEvent.Token0.Equals(_sniperConfig.LiquidityPairAddress, StringComparison.InvariantCultureIgnoreCase) ?
                pairCreatedEvent.Token1 : pairCreatedEvent.Token0;
            return await _bscWeb3.Eth.GetContract(_erc20Abi, otherPairAddress).GetFunction("symbol").CallAsync<string>();
        }

        public async Task<bool> CheckRugAsync(PairCreatedEvent pairCreatedEvent)
        {            
            if (pairCreatedEvent.Token0 != _sniperConfig.LiquidityPairAddress && pairCreatedEvent.Token1 != _sniperConfig.LiquidityPairAddress)
            {
                Serilog.Log.Logger.Warning("CheckRugAsync: Target liquidity pair not found for pair: {0} - {1}. Not bought", pairCreatedEvent.Token0, pairCreatedEvent.Token0);
                
                //results.Add(new RugCheckResult("CHECK_RUG_LIQUIDITY_PAIR", "LIQUIDITY_PAIR_NOT_FOUND", "Not bought", null, pairCreatedEvent.Symbol, false));
                //return results;
                return false;                            
            }

            var otherPairAddress = pairCreatedEvent.Token0.Equals(_sniperConfig.LiquidityPairAddress, StringComparison.InvariantCultureIgnoreCase) ?
                pairCreatedEvent.Token1 : pairCreatedEvent.Token0;
            var otherPairIdx = pairCreatedEvent.Token0.Equals(_sniperConfig.LiquidityPairAddress, StringComparison.InvariantCultureIgnoreCase) ?
                1 : 0;

            Task<bool>[] rugCheckerTasks = new Task<bool>[] {          
                RugdocCheck(otherPairAddress),
                SafuCheck(otherPairAddress),
                CheckContractVerified(otherPairAddress),
                CheckMinLiquidity(pairCreatedEvent, otherPairAddress, otherPairIdx)                                              
            };
            await Task.WhenAll(rugCheckerTasks);

            return (rugCheckerTasks.All(t => t.IsCompletedSuccessfully && t.Result));
            //return results;
                        
            /*
            Task<RugCheckResult>[] newRugCheckerTasks = new Task<RugCheckResult>[] {            
                RugdocCheck(otherPairAddress),
                SafuCheck(otherPairAddress),
                CheckContractVerified(otherPairAddress),
                CheckMinLiquidity(pairCreatedEvent, otherPairAddress, otherPairIdx)
            };
            await Task.WhenAll(newRugCheckerTasks);
            return newRugCheckerTasks.All(results);            
            */
        }

        private async Task<bool> CheckMinLiquidity(PairCreatedEvent pairCreatedEvent, string otherPairAddress, int otherPairIdx)
        {

            //TokenPair tp = _coinAndLiquidityHandler.GetTokenPairs(otherPairAddress);

            try {
                var response = await _httpClient.GetAsync(string.Format(RugdocCheckUrl, otherPairAddress));
                var _result = await _httpClient.GetAsync(string.Format(GetSourceUrl, otherPairAddress, _sniperConfig.BscScanApikey));
                var jObject = JObject.Parse(await _result.Content.ReadAsStringAsync());
                var innerResult = jObject["result"][0];
                var srcCode = innerResult.Value<string>("SourceCode");
                
                var sc = new SmartContract(srcCode, otherPairAddress, null, null, false);
                
                var currentPair = pairCreatedEvent.Pair;
                var reserves = await GetReserves(currentPair);
                var totalAmount = pairCreatedEvent.Token0.Equals(_sniperConfig.LiquidityPairAddress, StringComparison.InvariantCultureIgnoreCase) ? reserves.Reserve0 : reserves.Reserve1;

                var result = totalAmount >= Web3.Convert.ToWei(_sniperConfig.MinLiquidityAmount);
                var amountStr = Web3.Convert.FromWei(totalAmount).ToString();
                if (!result)
                {
                    Serilog.Log.Logger.Warning("CheckMinLiquidity: Not enough liquidity added to token {0}. Not buying. Only {1} liquidity added", otherPairAddress, amountStr);

                    //AJB
                    sc.Ok = false;
                    sc.Status = "Not enough liquidity for token! RugdocCheck - " + amountStr;
                    _smartContractList.Add(sc);                          
                    SmartContractOperations.SaveContracts(_smartContractList);

                    //results.Add(new RugCheckResult("CHECK_MIN_LIQUIDITY", "INSUFFICIENT_LIQUIDITY", sc.Status, null, otherPairAddress, sc.Ok));
                    //return new RugCheckResult("CHECK_MIN_LIQUIDITY", "INSUFFICIENT_LIQUIDITY", sc.Status, null, otherPairAddress, sc.Ok);
                    //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "INSUFFICIENT_LIQUIDITY", sc.Status, sc.Ok, null));
                    CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "INSUFFICIENT_LIQUIDITY", sc.Status, sc.Ok, null));

                    return false;
                    

                }
                else
                {
                    Serilog.Log.Logger.Information("CheckMinLiquidity: Min Liqudity check for {0} token passed. Liqudity amount: {1}", currentPair, amountStr);
                }

                if (_sniperConfig.MinimumPercentageOfTokenInLiquidityPool > 0)
                {
                    var ercContract = _bscWeb3.Eth.GetContract(_erc20Abi, otherPairAddress);
                    var balanceOfFunction = ercContract.GetFunction("balanceOf");
                    var tokenAmountInPool = otherPairIdx == 1 ? reserves.Reserve1 : reserves.Reserve0;
                    var deadWalletBalanceTasks = _sniperConfig.DeadWallets.Select(t => balanceOfFunction.CallAsync<BigInteger>(t)).ToList();
                    await Task.WhenAll(deadWalletBalanceTasks);
                    BigInteger deadWalletBalance = new BigInteger(0);
                    deadWalletBalanceTasks.ForEach(t => deadWalletBalance += t.Result);
                    var totalTokenAmount = await ercContract.GetFunction("totalSupply").CallAsync<BigInteger>();
                    if (totalTokenAmount == 0)
                    {
                        Serilog.Log.Logger.Error("CheckMinLiquidity: Token {0} contract is giving a invalid supply", otherPairAddress);

                        //AJB
                        sc.Ok = false;
                        sc.Status = "Token contract is giving an invalid supply";
                        _smartContractList.Add(sc);                    
                        SmartContractOperations.SaveContracts(_smartContractList);

                        //results.Add(new RugCheckResult("CHECK_MIN_LIQUIDITY", "CHECK_MIN_LIQUIDITY", sc.Status, null, otherPairAddress, sc.Ok));
                        //return new RugCheckResult("CHECK_MIN_LIQUIDITY", "TOKEN_CONTACT_RETURNING_INVALID_SUPPY", sc.Status, null, otherPairAddress, sc.Ok);

                        //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "CHECK_MIN_LIQUIDITY", sc.Status, sc.Ok, null));
                        CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "CHECK_MIN_LIQUIDITY", sc.Status, sc.Ok, null));

                        return false;
                        
                    }

                    var percentageInPool = new Fraction(tokenAmountInPool).Divide(totalTokenAmount).Multiply(100);
                    totalAmount -= deadWalletBalance;
                    result = ((decimal)percentageInPool) > _sniperConfig.MinimumPercentageOfTokenInLiquidityPool;

                    sc.Ok = true;
                    sc.Status = "Token: " + otherPairAddress + " Token Amount in Pool: " + tokenAmountInPool + " Total Supply: " + totalTokenAmount+ " Burned: " + deadWalletBalance.ToString()+" Total Percentage in pool: " + percentageInPool.ToDouble() + " Min Percentage Liquidity Check Status: " + result;                                                     
                    
                    Serilog.Log.Logger.Information("CheckMinLiquidity: Token {0} Token Amount in Pool: {1} Total Supply: {2} Burned {3} Total Percentage in pool: {4}% Min Percentage Liquidity Check Status: {5}", otherPairAddress, tokenAmountInPool, totalTokenAmount, deadWalletBalance.ToString(), percentageInPool.ToDouble(), result);                
            
                    //results.Add(new RugCheckResult("CHECK_MIN_LIQUIDITY", "MIN_PERCENTAGE_OF_TOKEN_IN_LIQUIDITY_POOL", sc.Status, null, otherPairAddress, sc.Ok));
                    //return new RugCheckResult("CHECK_MIN_LIQUIDITY", "MIN_PERCENTAGE_OF_TOKEN_IN_LIQUIDITY_POOL", sc.Status, null, otherPairAddress, sc.Ok);
                    //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "MIN_PERCENTAGE_OF_TOKEN_IN_LIQUIDITY_POOL", sc.Status, sc.Ok, null));
                    CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "MIN_PERCENTAGE_OF_TOKEN_IN_LIQUIDITY_POOL", sc.Status, sc.Ok, null));

                    return result;
                    
                }
                else 
                {                                        
                    sc.Ok = false;
                    sc.Status = "Token: " + otherPairAddress + ". Pool has insufficient liquidity";

                    //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "POOL_HAS_INSUFFICIENT_LIQUIDITY", sc.Status, sc.Ok, null));
                    CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "POOL_HAS_INSUFFICIENT_LIQUIDITY", sc.Status, sc.Ok, null));
                    
                    //results.Add(new RugCheckResult("CHECK_MIN_LIQUIDITY", "INSUFFICIENT_PERCENTAGE_OF_TOKEN_IN_LIQUIDITY_POOL", sc.Status, null, otherPairAddress, sc.Ok));
                    //return new RugCheckResult("CHECK_MIN_LIQUIDITY", "INSUFFICIENT_PERCENTAGE_OF_TOKEN_IN_LIQUIDITY_POOL", sc.Status, null, otherPairAddress, sc.Ok);
                    return false;
                }
                                            
            }
            catch (Exception e) {
                Serilog.Log.Logger.Error("CheckMinLiquidity: Error extracting information for {0} Error {1}", otherPairAddress, e.ToString());
                
                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "EXCEPTION_EXTRACTING_INFORMATION", e.ToString(), false, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "CHECK_MIN_LIQUIDITY", "EXCEPTION_EXTRACTING_INFORMATION", e.ToString(), false, null));

                //results.Add(new RugCheckResult("CHECK_MIN_LIQUIDITY", "EXCEPTION", "Error extracting information for " + otherPairAddress + e.ToString(), null, otherPairAddress, false));                //return new RugCheckResult("CHECK_MIN_LIQUIDITY", "EXCEPTION", "Error extracting information for " + otherPairAddress + e.ToString(), null, otherPairAddress, false);
                return false;                
            }

        }

        public Task<Reserves> GetReserves(string currentPair)
        {
            var pairContract = _bscWeb3.Eth.GetContract(_pairContractStr, currentPair);
            return pairContract.GetFunction("getReserves").CallDeserializingToObjectAsync<Reserves>();
        }

        public async Task<bool> CheckContractVerified(string otherPairAddress)
        {
            //TokenPair tp = _coinAndLiquidityHandler.GetTokenPairs(otherPairAddress);

            var result = await _httpClient.GetAsync(string.Format(GetSourceUrl, otherPairAddress, _sniperConfig.BscScanApikey));
            var jObject = JObject.Parse(await result.Content.ReadAsStringAsync());
            var innerResult = jObject["result"][0];
            var srcCode = innerResult.Value<string>("SourceCode");

            var sc = new SmartContract(srcCode, otherPairAddress, null, null, false);

            if(!_sniperConfig.CheckContractVerified)
            {
                sc.Ok = true;
                sc.Status = "Bsc contract is ok! CheckContractVerified disabled";
                _smartContractList.Add(sc);                
                SmartContractOperations.SaveContracts(_smartContractList);           

                //results.Add(new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "Not enabled", null, otherPairAddress, sc.Ok));
                //return new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "NOT_ENABLED", "Not enabled", null, otherPairAddress, sc.Ok);                

                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", sc.Status, sc.Ok, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", sc.Status, sc.Ok, null));

                return true;
                
            }

            if (innerResult["ABI"].Value<string>() == "Contract source code not verified")
            {
                Serilog.Log.Logger.Warning("CheckContractVerified: Bsc contract is not verified for token {0}", otherPairAddress);

                sc.Ok = false;
                sc.Status = "Bsc contract is not verified for token";
                _smartContractList.Add(sc);                
                SmartContractOperations.SaveContracts(_smartContractList);                
            
                //results.Add(new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_NOT_VERIFIED", sc.Status, null, otherPairAddress, sc.Ok));
                //return new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_NOT_VERIFIED", sc.Status, null, otherPairAddress, sc.Ok);

                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_NOT_VERIFIED", sc.Status, sc.Ok, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_NOT_VERIFIED", sc.Status, sc.Ok, null));

                return false;                
            }

            if (_sniperConfig.CheckRouterAddressInContract)
            {

                //if (!srcCode.Contains(_sniperConfig.PancakeswapRouterAddress) && !srcCode.Contains(_sniperConfig.V1PancakeswapRouterAddress))
                if (srcCode.ContainsCaseInsensitive(_sniperConfig.PancakeswapRouterAddress) && !srcCode.ContainsCaseInsensitive(_sniperConfig.V1PancakeswapRouterAddress))                
                {
                    Serilog.Log.Logger.Information("CheckContractVerified: Pancake swap router is invalid for token {0} ", otherPairAddress);                
                    
                    sc.Ok = false;
                    sc.Status = "Pancake swap router is invalid for token";
                    _smartContractList.Add(sc);                    
                    SmartContractOperations.SaveContracts(_smartContractList);

                    //results.Add(new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "PANCAKE_SWAP_ROUTER_INVALID", sc.Status, null, otherPairAddress, sc.Ok));
                    //return new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "PANCAKE_SWAP_ROUTER_INVALID", sc.Status, null, otherPairAddress, sc.Ok);

                    //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "PANCAKE_SWAP_ROUTER_INVALID", sc.Status, sc.Ok, null));
                    CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "PANCAKE_SWAP_ROUTER_INVALID", sc.Status, sc.Ok, null));

                    return false;                    
                }
            }

            //var containsRugCheckStrings = _sniperConfig.ContractRugCheckStrings.FirstOrDefault(t => srcCode.Contains(t));
            var containsRugCheckStrings = _sniperConfig.ContractRugCheckStrings.FirstOrDefault(t => srcCode.ContainsCaseInsensitive(t));
            if (!string.IsNullOrEmpty(containsRugCheckStrings))
            {
                sc.Ok = false;
                sc.Status = "Bsc contract does not contain RugCheckStrings";
                _smartContractList.Add(sc);
                //_smartContractList.Append<SmartContract>(sc);
                SmartContractOperations.SaveContracts(_smartContractList);                

                Serilog.Log.Logger.Warning("CheckContractVerified: Failed rug check for token {0}, contains string: {1}", otherPairAddress, containsRugCheckStrings);
                
                //results.Add(new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_DOES_NOT_CONTAIN_RUGCHECK_STRINGS", sc.Status, null, otherPairAddress, sc.Ok));
                //return new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_DOES_NOT_CONTAIN_RUGCHECK_STRINGS", sc.Status, null, otherPairAddress, sc.Ok);

                //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_DOES_NOT_CONTAIN_RUGCHECK_STRINGS", sc.Status, sc.Ok, null));
                CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_DOES_NOT_CONTAIN_RUGCHECK_STRINGS", sc.Status, sc.Ok, null));

                return false;
            }

            sc.Ok = true;
            sc.Status = "Bsc contract is ok!";
            _smartContractList.Add(sc);
            SmartContractOperations.SaveContracts(_smartContractList);                
            
            //results.Add(new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_OK", sc.Status, null, otherPairAddress, sc.Ok));
            //return new RugCheckResult("RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_OK", sc.Status, null, otherPairAddress, sc.Ok);

            //tp.TokenPairEvents.Add(new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_DOES_NOT_CONTAIN_RUGCHECK_STRINGS", sc.Status, sc.Ok, null));
            CoinAndLiquidityHandler.UpdateTokenPair(otherPairAddress, new TokenEvent(otherPairAddress, "RUGDOC_CHECK_CHECK_CONTACT_VERIFIED", "CONTRACT_DOES_NOT_CONTAIN_RUGCHECK_STRINGS", sc.Status, sc.Ok, null));

            return true;
            
        }


    }
}
