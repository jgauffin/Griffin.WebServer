using System;
using System.Net;
using FluentAssertions;
using Griffin.Net.Protocols.Http;
using Griffin.WebServer.Modules;
using NSubstitute;
using Xunit;

namespace Griffin.WebServer.Tests.Modules
{
    public class ErrorModuleTests
    {
        [Fact]
        public void SetErrorPage_should_move_stream_to_beginning_so_that_The_contents_can_Be_Served()
        {
            var context = Substitute.For<IHttpContext>();
            context.Response.Returns(new HttpResponse(HttpStatusCode.OK, "OK", "HTTP/1.1"));
            context.LastException = new InvalidOperationException();
            var errModule = new ErrorModule();
            errModule.BuildCustomErrorPage(x => "Error!");

            errModule.EndRequest(context);

            context.Response.Body.Position.Should().Be(0);
            context.Response.Body.Length.Should().Be(6);
        }
    }
}