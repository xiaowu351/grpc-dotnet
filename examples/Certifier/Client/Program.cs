using System;
using Grpc.Net.Client;
using Grpc.Core;
using Certify;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Reflection.V1Alpha;
using System.Diagnostics;
using System.Net.Http;
using System.IO;
using Google.Protobuf.WellKnownTypes;

namespace Client
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            //var channel = GrpcChannel.ForAddress("https://localhost:5001");
            //var client = new Certifier.CertifierClient(channel);

            await CallCertificateInfoAsync(false);

            await CallCertificateInfoAsync(true);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }

        private static async Task CallCertificateInfoAsync(bool includeClientCertificate)
        {
            try
            {
                Console.WriteLine($"Setting up HttpClient has certificate:{includeClientCertificate}");
                var httpClient = CreateHttpClient(includeClientCertificate);

                

                var callCredential = CallCredentials.FromInterceptor((context, metadata) =>
                {
                    metadata.Add("Authorization", $"Bearer {CreateCertificateInfo().GetRawCertDataString()}");
                    return Task.CompletedTask;
                });

                var channelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredential);
                var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { 
                    HttpClient = httpClient,
                    Credentials = channelCredentials
                });
                
                var client = new Certifier.CertifierClient(channel);
                Console.WriteLine("Sending gRPC call...");

                var certificateInfo = await client.GetCertificateInfoAsync(new Empty());
                Console.WriteLine($"Server received client certificate:{ certificateInfo.HasCertificate}");
                if (certificateInfo.HasCertificate)
                {
                    Console.WriteLine($"Client certificate name:{certificateInfo.Name}");
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"gRPC error from calling service:{ex.Status.Detail}");
                
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unexpected error calling service." + ex.Message);
                throw;
            }
        }
         
        private static HttpClient CreateHttpClient(bool includeClientCertificate)
        {
            var handler = new HttpClientHandler();

            if (includeClientCertificate)
            {
                
                var clientCertificate = CreateCertificateInfo(); 
                handler.ClientCertificates.Add(clientCertificate);
                //handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                //handler.ServerCertificateCustomValidationCallback = (reqeust, certificate, chain, error) => {
                //    // 参考下列文章配置
                //    // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-3.1
                //    reqeust.Headers.Add("X-ARR-ClientCert", certificate.GetRawCertDataString());
                //    Console.WriteLine(error);
                //    return true;
                //};

                
               
            }

            return new HttpClient(handler);
        }

        private static X509Certificate CreateCertificateInfo()
        {
            var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var certPath = Path.Combine(basePath, "Certs", "client.pfx");
            var clientCertificate = new X509Certificate2(certPath, "1111");
            return clientCertificate;
        }

    }
}
