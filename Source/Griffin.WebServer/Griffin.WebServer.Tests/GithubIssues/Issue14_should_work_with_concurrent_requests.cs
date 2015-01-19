using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.WebServer.Modules;
using Xunit;

namespace Griffin.WebServer.Tests.GithubIssues
{
    public class Issue14_should_work_with_concurrent_requests : IDisposable, IHttpModule
    {
        private HttpServer _server;
        ManualResetEvent _eventToTriggerBySecond = new ManualResetEvent(false);
        private int _counter = 0;

        public Issue14_should_work_with_concurrent_requests()
        {
            var moduleManager = new ModuleManager();
            moduleManager.Add(this);
            _server = new HttpServer(moduleManager);
            _server.Start(IPAddress.Any, 0);
        }

        [Fact]
        public void InvokeTwoRequests_Both_Should_succeed()
        {
            var httpMsg = @"GET / HTTP/1.0
Host:onetrueerror.com
Content-Length: 0

";
            var client1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client1.Connect(IPAddress.Loopback, _server.LocalPort);
            var client2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client2.Connect(IPAddress.Loopback, _server.LocalPort);


            client1.Send(Encoding.ASCII.GetBytes(httpMsg));
            Thread.Sleep(100); //give the first socket a chance so that our evt get signalled down
            client2.Send(Encoding.ASCII.GetBytes(httpMsg));

            var buf = new byte[65535];
            var secondRequestTimer = new Stopwatch();
            secondRequestTimer.Start();
            client2.Receive(buf);
            secondRequestTimer.Stop();

            secondRequestTimer.ElapsedMilliseconds.Should()
                .BeLessThan(500, "because it should complete before the timeout of the first request");
            _eventToTriggerBySecond.WaitOne(500).Should().BeTrue("because we want to verify that the method really was executed");
        }

        public void Dispose()
        {
            _server.Stop();
        }

        public void BeginRequest(IHttpContext context)
        {
            if (_counter == 0)
            {
                _counter = 1;
                //this should halt the second 
                //request if they were not executed concurrently
                Thread.Sleep(2000);
            }
            else
            {
                //
                _eventToTriggerBySecond.Set();
            }
        }

        public void EndRequest(IHttpContext context)
        {
            
        }
    }
}
