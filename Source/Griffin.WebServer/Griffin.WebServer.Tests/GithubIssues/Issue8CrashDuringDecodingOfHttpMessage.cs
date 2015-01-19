using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FluentAssertions;
using Griffin.WebServer.Modules;
using Xunit;

namespace Griffin.WebServer.Tests.GithubIssues
{
    public class Issue8CrashDuringDecodingOfHttpMessage : IDisposable
    {
        private readonly ManualResetEvent _messageReceivedEvent;
        private readonly HttpServer _server;

        public Issue8CrashDuringDecodingOfHttpMessage()
        {
            _messageReceivedEvent = new ManualResetEvent(false);
            object msg;

            var moduleManager = new ModuleManager();
            var myM = new MyModule(_messageReceivedEvent);
            moduleManager.Add(myM);
            _server = new HttpServer(moduleManager);
            _server.Start(IPAddress.Any, 8088);
        }

        public void Dispose()
        {
            _server.Stop();
        }

        [Fact]
        public void using_keep_alive_in_HTTP_10_should_not_crash_the_Server()
        {
            var MessageToSend =
                @"GET /?signature=1dfea26808d632903549c69d78558fce1c418405&echostr=5867553698596935317&timestamp=1365661332&nonce=1366146317 HTTP/1.0
ContentEncoding:
ContentLength:0
ContentType:
User-Agent:Mozilla 4.0
Accept: 
Name:Host
Value:58.215.164.183
Pragma:no-cache
Connection:Keep-Alive

";

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(IPAddress.Loopback, _server.LocalPort);
            client.Send(Encoding.ASCII.GetBytes(MessageToSend));

            _messageReceivedEvent.WaitOne(1000).Should().BeTrue();
        }

        public class MyModule : IWorkerModule
        {
            private readonly ManualResetEvent _messageReceivedEvent;

            public MyModule(ManualResetEvent messageReceivedEvent)
            {
                _messageReceivedEvent = messageReceivedEvent;
            }

            public void BeginRequest(IHttpContext context)
            {
            }

            public void EndRequest(IHttpContext context)
            {
            }

            public void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback)
            {
                var msg = context.Request;
                if (msg.HttpMethod == "GET")
                {
                    var byContent = Encoding.UTF8.GetBytes("welcome, my friend");
                    context.Response.Body = new MemoryStream();
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Body.Write(byContent, 0, byContent.Length);
                    context.Response.Body.Position = 0;
                }
                else if (msg.HttpMethod == "POST")
                {
                    if (msg.ContentLength > 0 && msg.Body != null)
                    {
                        var buff = new byte[msg.ContentLength];
                        msg.Body.Read(buff, 0, msg.ContentLength);
                        var codeM = Encoding.GetEncoding("utf-8");
                        var strContent = codeM.GetString(buff);
                    }
                }

                _messageReceivedEvent.Set();
            }
        }
    }
}