using System;
using Grpc.Net.Client;
using Grpc.Core;
using Ticket;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net.Http;
using System.Web;

namespace Client
{
    class Program
    {
        private const string Address = "https://localhost:5001";

        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress(Address);
            var client = new Ticketer.TicketerClient(channel);

            Console.WriteLine("gRPC Ticketer");
            Console.WriteLine();
            Console.WriteLine("Press a key:");
            Console.WriteLine("1: Get available tickets");
            Console.WriteLine("2: Purchase ticket");
            Console.WriteLine("3: Authenticate");
            Console.WriteLine("4: Exit");
            Console.WriteLine();

            string? token = null;

            var exiting = false;
            while (!exiting)
            {
                var consoleKeyInfo = Console.ReadKey(intercept: true);
                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        await GetAvailableTicketsAsync(client);
                        break;
                    case '2':
                        await PurchaseTicketAsync(client, token);
                        break;
                    case '3':
                        token = await AuthenticateAsync();
                        break;
                    case '4':
                        exiting = true;
                        break;
                    default:
                        break;
                }
            }

            //await BidirectionalStreamingExample(client);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }


        private static async Task<string> AuthenticateAsync()
        {
            Console.WriteLine($"Authenticating as {Environment.UserName}...");
            var httpclient = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Address}/token?name={HttpUtility.UrlEncode(Environment.UserName)}"),
                Method = HttpMethod.Get,
                Version = new Version(2, 0)
            };
            var tokenResponse = await httpclient.SendAsync(request);
            tokenResponse.EnsureSuccessStatusCode();
            var token = await tokenResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Successfully authenticated.");
            return token;
        }

        private static async Task GetAvailableTicketsAsync(Ticketer.TicketerClient client)
        {
            Console.WriteLine("Getting available ticket count...");
            var response = await client.GetAvailableTicketsAsync(new Google.Protobuf.WellKnownTypes.Empty());
            Console.WriteLine($"Available ticket count:{response.Count}");
        }

        private static async Task PurchaseTicketAsync(Ticketer.TicketerClient client, string? token)
        {
            Console.WriteLine("Purchase ticket....");
            try
            {
                Metadata? headers = null;
                if (token != null)
                {
                    headers = new Metadata();
                    headers.Add("Authorization", $"Bearer {token}");
                }

                var response = await client.BuyTicketsAsync(new BuyTicketsRequest { Count = 1 }, headers);
                if (response.Success)
                {
                    Console.WriteLine("Purchase successful.");
                }
                else
                {
                    Console.WriteLine("Purchase failed. No tickets available.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error purchasing ticket." + Environment.NewLine + ex.ToString());
            }
        }
    }
}
