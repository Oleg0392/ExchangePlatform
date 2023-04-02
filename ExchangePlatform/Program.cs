using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangePlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*IHost host = CreateHostBuilder(args).Build();
            var provider = host.Services.GetService(typeof(ConfigurationProvider));
            if (provider is IConfigurationProvider) QueryManager.ConfigurationProvider = (IConfigurationProvider)provider;
            host.Run();*/
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
