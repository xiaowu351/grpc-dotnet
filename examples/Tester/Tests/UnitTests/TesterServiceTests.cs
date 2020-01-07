using NUnit.Framework;
using System.Threading.Tasks;
using Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Grpc.Core;
using Tests.UnitTests.Helpers;
using System.Threading;
using Test;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    public class TesterServiceTests
    {


        [Test]
        public async Task SayHelloUnaryTest()
        {
            // Arrange
            var service = new TesterService(NullLoggerFactory.Instance.CreateLogger<TesterService>());

            // Act 
            var response = await service.SayHelloUnary(new Test.HelloRequest { Name = "Joe" }, TestServerCallContext.Create());

            //Assert
            Assert.AreEqual("Hello Joe", response.Message);
        }

        [Test]
        public async Task SayHelloServerStreamingTest()
        {
            // Arrange
            var service = new TesterService(NullLoggerFactory.Instance.CreateLogger<TesterService>());

            var cts = new CancellationTokenSource();
            var callContext = TestServerCallContext.Create(cancellationToken: cts.Token);
            var responseStream = new TestServerStreamWriter<HelloReply>(callContext);

            // Act
            var call = service.SayHelloServerStreaming(new Test.HelloRequest { Name = "Joe" }, responseStream, callContext);

            //Assert
            Assert.IsFalse(call.IsCompletedSuccessfully, "Method should run until cancelled.");
            cts.Cancel();

            await call;
            responseStream.Complete();

            var allMessages = new List<HelloReply>();
            await foreach (var message in responseStream.ReadAllAsync())
            {
                allMessages.Add(message);
            }

            Assert.GreaterOrEqual(allMessages.Count, 1);

            Assert.AreEqual("How are you Joe? 1", allMessages[0].Message);
        }

        [Test]
        public async Task SayHelloClientStreamingTest()
        {
            //Arrange
            var service = new TesterService(NullLoggerFactory.Instance.CreateLogger<TesterService>());
            var callContext = TestServerCallContext.Create();
            var requestStream = new TestAsyncStreamReader<HelloRequest>(callContext);

            //Act
            var call = service.SayHelloClientStreaming(requestStream, callContext);
            requestStream.AddMessage(new HelloRequest { Name="James" });
            requestStream.AddMessage(new HelloRequest { Name = "Jo" });
            requestStream.AddMessage(new HelloRequest { Name = "Lee" });
            requestStream.Complete();

            //Assert
            var response = await call;
            Assert.AreEqual("Hello James, Jo, Lee",response.Message);

        }
        
        [Test]
        public async Task SayHelloBidirectionStreamingTest()
        {
            // Arrange
            var service = new TesterService(NullLoggerFactory.Instance.CreateLogger<TesterService>());

            var callContext = TestServerCallContext.Create();
            var requestStream = new TestAsyncStreamReader<HelloRequest>(callContext);
            var responseStream = new TestServerStreamWriter<HelloReply>(callContext);

            //Act
            var call = service.SayHelloBidirectionalStreaming(requestStream, responseStream, callContext);
            //Assert

            requestStream.AddMessage(new HelloRequest { Name = "James" });
            Assert.AreEqual("Hello James",(await responseStream.ReadNextAsync())!.Message);

            requestStream.AddMessage(new HelloRequest { Name = "Jo"});
            Assert.AreEqual("Hello Jo",(await responseStream.ReadNextAsync())!.Message);

            requestStream.Complete();

            await call;
            responseStream.Complete();

            Assert.IsNull(await responseStream.ReadNextAsync());
        }
    }
}