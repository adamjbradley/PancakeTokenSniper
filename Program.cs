using BscTokenSniper.Handlers;
using BscTokenSniper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Configuration;

namespace BscTokenSniper
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            var config = hostContext.Configuration;
            
            var loggerConfig = new LoggerConfiguration()
                  .ReadFrom.Configuration(config);

            var logger = loggerConfig.CreateLogger()
                .ForContext(MethodBase.GetCurrentMethod().DeclaringType);
            Log.Logger = logger;

            logger.Information("Running BSC Token Sniper with Args: @{args}", args);
            services.AddHttpClient();
            services.AddSingleton<TradeHandler>();
            services.AddSingleton<RugHandler>();
            services.Configure<SniperConfiguration>(config.GetSection("SniperConfiguration"));
            services.AddHostedService<SniperService>();
            //services.AddScoped<IImplementation, NewImplementation>();

            // From https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
            services.AddDbContext<PersistenceContext>(options => options.UseNpgsql("Host=192.168.1.120;Database=iiprod_db;Username=postgres;Password=postgres"));            
        });

    }
}
