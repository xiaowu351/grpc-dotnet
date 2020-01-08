using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Progress;
namespace Server
{
    public class ProgressorService:Progressor.ProgressorBase
    {
        private readonly ILogger _logger;
        
        public ProgressorService( ILoggerFactory loggerFactory){
             
            _logger = loggerFactory.CreateLogger<ProgressorService>();
        }

        public override async Task RunHistory(Empty request, IServerStreamWriter<HistoryResponse> responseStream, ServerCallContext context)
        {
            var bigDatas = await File.ReadAllLinesAsync("BigData.txt");
            var processedBigData = new List<string>();

            for (int i = 0; i < bigDatas.Length; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.2));

                var data = bigDatas[i];

                _logger.LogInformation("Adding {data}",data);
                processedBigData.Add(data);

                var progress = (i + 1) / (double)bigDatas.Length;
                await responseStream.WriteAsync(new HistoryResponse {
                      Progress = Convert.ToInt32(progress*100)
                });
            }

            _logger.LogInformation("History complete. Returning {Count} BigDatas",processedBigData.Count);
            var historyResult = new HistoryResult();
            historyResult.Items.Add(processedBigData);

            await responseStream.WriteAsync(new HistoryResponse { 
                Result = historyResult
            });  
        }
    }
}