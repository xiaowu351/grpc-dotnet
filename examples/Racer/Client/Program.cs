using System;
using Grpc.Net.Client;
using Grpc.Core;
using Race;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Client
{
    class Program
    {
        static TimeSpan RaceDuration = TimeSpan.FromSeconds(3);
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Racer.RacerClient(channel);


            Console.WriteLine("Press any key to start race...");
            //Console.ReadKey();

            await BidirectionalStreamingExample(client);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }

        
        private static async Task BidirectionalStreamingExample(Racer.RacerClient client)
        {
            var headers = new Metadata { new Metadata.Entry("race-duration", RaceDuration.ToString()) };
            Console.WriteLine("Ready, set ,go!");
            using (var call = client.ReadySetGo(new CallOptions(headers)))
            {
                RaceMessage? lastMessageReceived = null;
                var readTask = Task.Run(async () =>
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                    {
                        lastMessageReceived = message;
                    }
                });

                var sw = Stopwatch.StartNew();
                var sent = 0;
                while (sw.Elapsed < RaceDuration)
                {
                    await call.RequestStream.WriteAsync(new RaceMessage { Count = ++sent });
                }

                await call.RequestStream.CompleteAsync();
                await readTask;

                Console.WriteLine($"Messages sent: {sent}");
                Console.WriteLine($"Messages received: {lastMessageReceived?.Count ?? 0}");
            }
        }
         
    }
}
