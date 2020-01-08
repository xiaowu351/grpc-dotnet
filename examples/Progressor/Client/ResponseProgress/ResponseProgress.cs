using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Progress;

namespace Client.ResponseProgress
{

    public static class GrpcProgress
    {
        public static ResponseProgress<TResult, TProgress> Create<TResult, TProgress>(
            IAsyncStreamReader<IProgressMessage<TResult, TProgress>> streamReader,
            IProgress<TProgress>? progress = null)
        {
            return new ResponseProgress<TResult, TProgress>(streamReader, progress);
        }
    }

    public class ResponseProgress<TResult, TProgress> : Progress<TProgress>
    {
        private readonly IAsyncStreamReader<IProgressMessage<TResult, TProgress>> _streamReader;
        private readonly IProgress<TProgress>? _progress;
        private readonly Task<TResult> _resultTask;
        

        public ResponseProgress(IAsyncStreamReader<IProgressMessage<TResult, TProgress>> streamReader, IProgress<TProgress>? progress = null)
        {
            _streamReader = streamReader;
            _progress = progress;

            // Start reading from the stream in the background, updating IProgress with values from the server.
            // When the result is returned set it into the task complete source.
            _resultTask = Task.Run<TResult>(async () =>
            {
                await foreach (var item in _streamReader.ReadAllAsync())
                {
                    if (item.IsProgress)
                    {
                        _progress?.Report(item.Progress);
                        OnReport(item.Progress);
                    }
                    if (item.IsResult)
                    {
                        return item.Result;
                    }
                }

                throw new Exception("Call completed without a result.");
            });
        } 
        public TaskAwaiter<TResult> GetAwaiter()
        {
            return _resultTask.GetAwaiter();
        }
    }
}
