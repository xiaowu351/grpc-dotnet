using System;
using Grpc.Net.Client;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Health.V1;

namespace Client
{
    class Program
    {
        static Random RNG = new Random();
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Health.HealthClient(channel);


            Console.WriteLine("Watching health status");
            Console.WriteLine("Press any key to exit...");


            var response = await client.CheckAsync(new HealthCheckRequest { Service = "T1" });

            Console.WriteLine($"check call result Service is  StatusCode:{response.Status}");

            var cts = new CancellationTokenSource();
            var call = client.Watch(new HealthCheckRequest { Service = "T1" }, cancellationToken: cts.Token);
            var watchTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"{DateTime.Now}: Service is {message.Status}");
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {

                    Console.WriteLine("Health is cancel.");
                }
            });



            Console.ReadKey();
            Console.WriteLine("watch cancel Finished");
            cts.Cancel();
            
            await watchTask;
        }


    }
}
