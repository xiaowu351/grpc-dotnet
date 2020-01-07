using System;
using Grpc.Net.Client;
using Grpc.Core;
using Greet;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Reflection.V1Alpha;
using System.Diagnostics;

namespace Client
{
    class Program
    {
        static Random RNG = new Random();
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ServerReflection.ServerReflectionClient(channel);

            var response = await SingleRequestAsync(client, new ServerReflectionRequest {
                ListServices = ""
            });

            Console.WriteLine("Services:");
            foreach (var item in response.ListServicesResponse.Service)
            {
                Console.WriteLine("-"+item.Name);
            }
             

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }

        static async Task<ServerReflectionResponse> SingleRequestAsync(ServerReflection.ServerReflectionClient client,ServerReflectionRequest request)
        {
            var call = client.ServerReflectionInfo();
            await call.RequestStream.WriteAsync(request);
            Debug.Assert(await call.ResponseStream.MoveNext());

            var response = call.ResponseStream.Current;
            await call.RequestStream.CompleteAsync();
            return response;
        }

        
    }
}
