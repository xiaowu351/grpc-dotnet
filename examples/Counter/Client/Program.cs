using System;
using Grpc.Net.Client;
using Grpc.Core;
using Count;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static Random RNG = new Random();
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Counter.CounterClient(channel);


            await UnaryCallExample(client);

            await ClientStreamingCallExample(client);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }

        static async Task UnaryCallExample(Counter.CounterClient client)
        {
            var reply = await client.IncrementCountAsync(new Google.Protobuf.WellKnownTypes.Empty());
            Console.WriteLine("Count:" + reply.Count);
        }

        /// <summary>
        /// Client streaming call
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static async Task ClientStreamingCallExample(Counter.CounterClient client)
        {
            using (var call = client.AccumulateCount())
            {
                for (var i = 0; i < 3; i++)
                {
                    var count = RNG.Next(5);
                    Console.WriteLine($"Accumulating with {count}");
                    await call.RequestStream.WriteAsync(new CounterRequest { Count = count });
                    await Task.Delay(2000);
                }

                await call.RequestStream.CompleteAsync();

                var response = await call;
                Console.WriteLine($"Count:{response.Count}");
            }
        }
    }
}
