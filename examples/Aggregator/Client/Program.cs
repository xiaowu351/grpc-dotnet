using System;
using Grpc.Net.Client;
using Grpc.Core;
using Aggregate;
using System.Threading.Tasks;
using System.Threading;

namespace Client
{
    class Program
    {
        static Random RNG = new Random();
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new Aggregator.AggregatorClient(channel);

            await ServerStreamCallExample(client);

            await ClientStreamingCallExample(client);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }


        private static async Task ServerStreamCallExample(Aggregator.AggregatorClient client)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3.5));
            using (var replies = client.SayHellos(new Greet.HelloRequest { Name = "AggregatorClient" },
                cancellationToken: cts.Token))
            {
                try
                {
                    await foreach (var message in replies.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine("Greeting:" + message.Message);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Stream cancel.");
                }

            }
        }

        private static async Task ClientStreamingCallExample(Aggregator.AggregatorClient client)
        {
            using (var call = client.AccumulateCount())
            {
                for (int i = 0; i < 3; i++)
                {
                    var count = RNG.Next(5);
                    Console.WriteLine($"Accumulation with {count}");
                    await call.RequestStream.WriteAsync(new Count.CounterRequest { Count = count });
                    await Task.Delay(2000);
                }

                await call.RequestStream.CompleteAsync();
                var response = await call;
                Console.WriteLine($"Count:{response.Count}");
            }
        }


    }
}
