using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Race;



namespace Server
{
    public class RacerService : Racer.RacerBase
    {
        private readonly ILogger<RacerService> _logger;
        public RacerService(ILogger<RacerService> logger)
        {
            _logger = logger;
        } 

        public override async Task ReadySetGo(Grpc.Core.IAsyncStreamReader<RaceMessage> requestStream, Grpc.Core.IServerStreamWriter<RaceMessage> responseStream, Grpc.Core.ServerCallContext context){
            var raceDuration = TimeSpan.Parse(context.RequestHeaders.Single(h=>h.Key == "race-duration").Value);
            RaceMessage? lastMessageReceived = null;
            var readTask = Task.Run(async ()=> {
                await foreach (var message in requestStream.ReadAllAsync())
                {
                    lastMessageReceived = message;
                }
            });

            var sw = Stopwatch.StartNew();
            var sent = 0;
            while (sw.Elapsed< raceDuration)
            { 
               await responseStream.WriteAsync(new RaceMessage { Count = ++sent});
            }

            await readTask;
        }
    }
}
