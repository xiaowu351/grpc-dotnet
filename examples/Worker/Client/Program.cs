using System;
using Grpc.Net.Client;
using Grpc.Core;
using Count;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace Client
{
    class Program
    {
        static Random RNG = new Random();
        static void  Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
           return  Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    services.AddHostedService<Worker>();
                    services.AddGrpcClient<Counter.CounterClient>(options=> {
                        options.Address = new Uri("https://localhost:5001");
                    });
                });
        }
        
    }
}
