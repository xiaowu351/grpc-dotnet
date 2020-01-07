using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Tests.UnitTests.Helpers
{
    public class TestServerStreamWriter<T> : IServerStreamWriter<T> where T : class
    {
        private readonly ServerCallContext _serverCallContext;
        private readonly Channel<T> _chanel;
        public WriteOptions WriteOptions { get ; set; }


        public TestServerStreamWriter(ServerCallContext serverCallContext)
        {
            _serverCallContext = serverCallContext;
            _chanel = Channel.CreateUnbounded<T>();
        }

        public void Complete()
        {
            _chanel.Writer.Complete();
        }

        public IAsyncEnumerable<T> ReadAllAsync()
        {
            return _chanel.Reader.ReadAllAsync();
        }

        public async Task<T> ReadNextAsync() 
        {
            if(await _chanel.Reader.WaitToReadAsync())
            {
                _chanel.Reader.TryRead(out var message);
                return message;
            }
            else
            {
                return null;
            }
        }

        public Task WriteAsync(T message)
        {
            if (_serverCallContext.CancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(_serverCallContext.CancellationToken);
            }

            if (!_chanel.Writer.TryWrite(message))
            {
                throw new InvalidOperationException("Unable to write message.");
            }
            return Task.CompletedTask;
        }
    }
}
