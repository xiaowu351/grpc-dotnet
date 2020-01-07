using System;
using Grpc.Net.Client;
using Grpc.Core;
using Greet;
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
            var client = new Greeter.GreeterClient(channel);


            await UnaryCallExample(client);

            await ServerStreamingCallExample(client);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }

        static async Task UnaryCallExample(Greeter.GreeterClient client)
        {
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
            Console.WriteLine("Greeting:" + reply.Message);
        }

        /// <summary>
        /// Client streaming call
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static async Task ServerStreamingCallExample(Greeter.GreeterClient client)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3.5));
            using (var call = client.SayHellos(new HelloRequest { Name = "GreeterClient" }, cancellationToken: cts.Token))
            {
                try{
                    await foreach(var message in call.ResponseStream.ReadAllAsync()){
                        Console.WriteLine("Greeting:"+ message.Message);
                    }
                }catch(RpcException ex) when(ex.StatusCode == StatusCode.Cancelled){
                    Console.WriteLine("Steam cancelled.");
                }
            }
        }
    }
}
