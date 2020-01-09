using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.FunctionalTests.Helpers
{
    public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception exception);
    public class GrpcTestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;
        private readonly IHost _host;

        public event LogMessage? LoggedMessage;

        public LoggerFactory LoggerFactory { get; }

        public HttpClient Client { get; }

        public GrpcTestFixture(Action<IServiceCollection>? initialConfigureServices)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
            {
                LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
            }));

            var builder = new HostBuilder().ConfigureServices(services =>
            {
                initialConfigureServices?.Invoke(services);
                services.AddSingleton<ILoggerFactory>(LoggerFactory);
            }).ConfigureWebHostDefaults(webHost =>
            {
                webHost.UseTestServer()
                       .UseStartup<TStartup>();
            });

            _host = builder.Start();
            _server = _host.GetTestServer();
            // Need to set the response version to 2.0.
            // dotnet core 3.1 fix bug, IHost.GetTestClient() 即可
            //var responseVersionHandler = new ResponseVersionHandler();
            //responseVersionHandler.InnerHandler = _server.CreateHandler();

            //var client = new HttpClient(responseVersionHandler);
            //client.BaseAddress = new Uri("http://localhost");

            //Client = client;

            Client = _host.GetTestClient();//new HttpClient { BaseAddress=new Uri("http://localhost") };
        
        
        }

        public void Dispose()
        {
            Client.Dispose();
            _host.Dispose();
            _server.Dispose();
        }



        public IDisposable GetTestContext()
        {
            return new GrpcTestContext<TStartup>(this);
        }


        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;

                return response;
            }
        }
    }
}
