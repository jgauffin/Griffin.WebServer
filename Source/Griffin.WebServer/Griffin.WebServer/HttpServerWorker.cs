using System;
using System.Net;
using Griffin.Networking.Protocol.Http;
using Griffin.Networking.Protocol.Http.Protocol;
using Griffin.WebServer.Modules;

namespace Griffin.WebServer
{
    /// <summary>
    /// One instance per HTTP connection
    /// </summary>
    /// <remarks></remarks>
    public class HttpServerWorker : HttpService
    {
        private readonly WorkerConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerWorker" /> class.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <param name="configuration">The configuration.</param>
        public HttpServerWorker(IPEndPoint remoteEndPoint, WorkerConfiguration configuration)
            : base(configuration.BufferSliceStack)
        {
            if (remoteEndPoint == null) throw new ArgumentNullException("remoteEndPoint");
            if (configuration == null) throw new ArgumentNullException("configuration");
            RemoteEndPoint = remoteEndPoint;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets end point that the client connected from
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
        }

        /// <summary>
        /// We've received a HTTP request.
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <exception cref="System.ArgumentNullException">message</exception>
        public override void OnRequest(IRequest request)
        {
            var context = new WebServer.HttpContext
                {
                    Application = _configuration.Application,
                    Items = new MemoryItemStorage(),
                    Request = request,
                    Response = request.CreateResponse(HttpStatusCode.OK, "Okey dokie")
                };

            context.Response.AddHeader("X-Powered-By",
                                       "Griffin.Networking (http://github.com/jgauffin/griffin.networking)");


            _configuration.ModuleManager.InvokeAsync(context, SendResponse);
        }

        /// <summary>
        /// Callback from the module manager
        /// </summary>
        protected virtual void SendResponse(IAsyncModuleResult result)
        {
            Send(result.Context.Response);
        }
    }
}