using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test;
using Grpc.Net.Client;
using Grpc.Core;

namespace Tests.FunctionalTests
{
    [TestFixture]
    public class TesterServiceTests : FunctionalTestBase
    {

        [Test]
        public async Task SayHelloUnaryTest()
        {
            // Arrange 
            var client = new Tester.TesterClient(Channel);

            //Act
            var response = await client.SayHelloUnaryAsync(new HelloRequest { Name = "Joe" });

            //Assert
            Assert.AreEqual("Hello Joe", response.Message);
        }

        [Test]
        public async Task SayHelloClientStreamingTest()
        {
            // Arrange 
            var client = new Tester.TesterClient(Channel);
            var names = new[] { "James", "Jo", "Lee" };
            HelloReply response;

            //Act
            using (var call = client.SayHelloClientStreaming())
            {
                foreach (var name in names)
                {
                    await call.RequestStream.WriteAsync(new HelloRequest {Name=name });
                }

                await call.RequestStream.CompleteAsync();
                response = await call;

            }

            //Assert
            Assert.AreEqual("Hello James, Jo, Lee", response.Message);
        }

        [Test]
        public async Task SayHelloServerStreamingTest()
        {
            // Arrange
            var client = new Tester.TesterClient(Channel);
            var cts = new CancellationTokenSource();
            var hasMessage = false;
            var callCancelled = false;
            //Act
            using (var call = client.SayHelloServerStreaming(new HelloRequest { Name="Joe"},cancellationToken:cts.Token))
            {
                try
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                    {
                        hasMessage = true;
                        cts.Cancel();
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {

                    callCancelled = true;
                }
                
            }

            Assert.IsTrue(hasMessage);
            Assert.IsTrue(callCancelled);
        }

        [Test]
        public async Task SayHelloBidirectionStreamingTest()
        {
            // Arrange
            var client = new Tester.TesterClient(Channel);
            var names = new[] { "James", "Jo", "Lee" };
            var messages = new List<string>();
            //Act
            using (var call = client.SayHelloBidirectionalStreaming())
            {
                foreach (var name in names)
                {
                    await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
                    Assert.IsTrue(await call.ResponseStream.MoveNext());
                    messages.Add(call.ResponseStream.Current.Message);
                }

                await call.RequestStream.CompleteAsync();

            }

            //Assert
            Assert.GreaterOrEqual(messages.Count,1);
            Assert.AreEqual("Hello James", messages[0]);
        }
    }
}
