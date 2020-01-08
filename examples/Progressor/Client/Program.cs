using System;
using Grpc.Net.Client;
using Grpc.Core;
using Progress;
using System.Threading.Tasks;
using Client.ResponseProgress;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;

namespace Client
{
    class Program
    {
        static Random RNG = new Random();
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Progressor.ProgressorClient(channel);

            var progress = new Progress<int>(i => Console.WriteLine($"Progress: {i}%"));

            await ServerStreamingCallExample(client, progress);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit ...");

            Console.ReadKey();
        }

        //private static ResponseProgress<HistoryResult,int> ServerStreamingCallExample(Progressor.ProgressorClient client,IProgress<int> progress)
        //{
        //    var call = client.RunHistory(new Google.Protobuf.WellKnownTypes.Empty());
        //    var historyResponse = call.ResponseStream;//as IAsyncStreamReader<IProgressMessage<HistoryResult, int>>;
        //    return GrpcProgress.Create(historyResponse, progress);
        //}

        //private static ResponseProgress<HistoryResult, int> ServerStreamingCallExample(
        //    Progressor.ProgressorClient client,
        //    IProgress<int> progress)
        //{
        //    var call = client.RunHistory(new Empty());

        //    Debug.Assert(call.ResponseStream is IAsyncStreamReader<HistoryResponse>, "response stream historyResponse");

        //    var result = call.ResponseStream is IAsyncStreamReader<IProgressMessage<HistoryResult, int>>;
        //    Debug.Assert(result, "response stream IAsyncStreamReader<IProgressMessage<HistoryResult, int>>");


        //    //if(call.ResponseStream is IAsyncStreamReader<IProgressMessage<HistoryResult, int>>)
        //    //{
        //    //    Console.WriteLine("response stream IAsyncStreamReader<IProgressMessage<HistoryResult, int>>");
        //    //}

        //    //return new ResponseProgress<HistoryResult, int>(call.ResponseStream as IAsyncStreamReader<IProgressMessage<HistoryResult, int>>, progress);


        //}

        private static async Task ServerStreamingCallExample(
            Progressor.ProgressorClient client,
            IProgress<int> progress)
        {
            var call = client.RunHistory(new Empty());

            var resultTask = Task.Run(async () =>
            {
                HistoryResult result = null;
                await foreach (var item in call.ResponseStream.ReadAllAsync())
                {
                    var progressMessage = item as IProgressMessage<HistoryResult, int>;
                    if(progressMessage == null)
                    {
                        Console.WriteLine("Server write is not Progress Message");
                        continue;
                    }
                    if (progressMessage.IsProgress)
                    {
                        progress?.Report(progressMessage.Progress);
                        continue;
                        
                    }
                    if (progressMessage.IsResult)
                    {
                        result = progressMessage.Result;
                    }
                }

                //throw new Exception("Call completed without a result.");
                return result;

                 
            });

            var result = await resultTask;
            if(result == null)
            {
                Console.WriteLine("not result.");
                return;
            }
            Console.WriteLine("Preparing results...");
            await Task.Delay(TimeSpan.FromSeconds(2));

            foreach (var item in result.Items)
            {
                Console.WriteLine(item);
            }
        }
    }
}
