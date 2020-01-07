using Count;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Counter.CounterClient _counterClient;
        private readonly Random _random;
        private AsyncClientStreamingCall<CounterRequest, CounterReply>? _clientStreamingCall;

        public Worker(ILogger<Worker> logger, Counter.CounterClient client)
        {
            _logger = logger;
            _counterClient = client;
            _random = new Random();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Staring client streaming call at :{time}", DateTimeOffset.Now);
            _clientStreamingCall = _counterClient.AccumulateCount();

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(_clientStreamingCall != null);

            _logger.LogInformation("Finishing call at:{time}", DateTimeOffset.Now);
            await _clientStreamingCall.RequestStream.CompleteAsync();

            var response = await _clientStreamingCall;

            _logger.LogInformation("Total count:{count}", response.Count);

            //Console.WriteLine("Shutting down");
            //Console.WriteLine("Press any key to exit ...");
            //Console.ReadKey();

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Debug.Assert(_clientStreamingCall != null);

            while (!stoppingToken.IsCancellationRequested)
            {
                var count = _random.Next(1, 10);

                _logger.LogInformation("Sending count {count} at:{time}", count, DateTimeOffset.Now);
                await _clientStreamingCall.RequestStream.WriteAsync(new CounterRequest { Count = count });
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
