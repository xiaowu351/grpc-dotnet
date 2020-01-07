using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Greet;
namespace Server
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"sending hello to {request.Name}");
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task SayHellos(HelloRequest request, Grpc.Core.IServerStreamWriter<HelloReply> responseStream, Grpc.Core.ServerCallContext context)
        {
            var i = 0;
            while(!context.CancellationToken.IsCancellationRequested){
                 var message = $"How are you {request.Name}? {++i}";
                 _logger.LogInformation($"Sending greeting {message}.");
                 await responseStream.WriteAsync(new HelloReply{Message = message});   
                 await Task.Delay(1000);
            }
        }
    }
}
