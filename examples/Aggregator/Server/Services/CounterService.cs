using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Count;
namespace Server
{
    public class CounterService:Counter.CounterBase
    {
        private readonly ILogger _logger;
        private readonly IncrementingCounter _counter;
        public CounterService(IncrementingCounter counter,ILoggerFactory loggerFactory){
            _counter = counter;
            _logger = loggerFactory.CreateLogger<CounterService>();
        }

        public override Task<CounterReply> IncrementCount(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context){
            _logger.LogInformation("Increment count by 1");
            _counter.Increment(1);
            return Task.FromResult(new CounterReply{ Count = _counter.Count});
        }

        public override async Task<CounterReply> AccumulateCount(IAsyncStreamReader<CounterRequest> requestStream, ServerCallContext context){
            await foreach(var message in requestStream.ReadAllAsync()){
                _logger.LogInformation($"Incrementing count by {message.Count}");
                _counter.Increment(message.Count);
            }
            
            return new CounterReply{ Count = _counter.Count};
        }
    }
}