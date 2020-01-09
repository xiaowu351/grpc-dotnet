using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests
{
    public class FunctionalTestBase
    {

        private GrpcChannel? _channel;
        private IDisposable _testContext;

        protected GrpcTestFixture<Server.Startup> Fixture { get; private set; } = default;

        protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

        protected GrpcChannel Channel => _channel ??= CreateChannel();

        protected GrpcChannel CreateChannel()
        {
            return GrpcChannel.ForAddress(Fixture.Client.BaseAddress,new GrpcChannelOptions { 
                LoggerFactory = LoggerFactory,
                HttpClient = Fixture.Client
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {

        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Fixture = new GrpcTestFixture<Server.Startup>(ConfigureServices);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Fixture.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _testContext = Fixture.GetTestContext();
        }

        public void TearDown()
        {
            _testContext?.Dispose();
            _channel = null;
        }
    }
}
