using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Tests.FunctionalTests.Helpers
{
    public class GrpcTestContext<TStartup> : IDisposable where TStartup : class
    {
        private readonly ExecutionContext _executionContext;
        private readonly Stopwatch _stopwatch;
        private readonly GrpcTestFixture<TStartup> _fixture;

        public GrpcTestContext(GrpcTestFixture<TStartup> fixture)
        {
            _executionContext = ExecutionContext.Capture();
            _stopwatch = Stopwatch.StartNew();
            _fixture = fixture;
            _fixture.LoggedMessage += WriteMessage;
        }

        private void WriteMessage(LogLevel logLevel, string category, EventId eventId, string message, Exception exception)
        {
            ExecutionContext.Run(_executionContext, s =>
            {
                Console.WriteLine($"{_stopwatch.Elapsed.TotalSeconds:N3}s {category} - {logLevel} : {message}");
            }, null);
        }


        public void Dispose()
        {
            _executionContext?.Dispose();
        }
    }
}
