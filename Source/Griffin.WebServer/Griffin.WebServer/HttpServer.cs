using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Griffin.Net;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Serializers;
using Griffin.WebServer.Modules;
using HttpListener = Griffin.Net.Protocols.Http.HttpListener;


namespace Griffin.WebServer
{
    /// <summary>
    ///     Default HTTP Server implementation
    /// </summary>
    /// <remarks>This implementation uses modules for everything</remarks>
    public class HttpServer
    {
        private readonly BufferSlicePool _bufferSlicePool;
        private readonly IModuleManager _moduleManager;
        private readonly ChannelTcpListenerConfiguration _configuration;
        private HttpListener _listener;
        

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpServer" /> class.
        /// </summary>
        /// <param name="moduleManager">The modules are used to process the HTTP requests. You need to specify at least one.</param>
        public HttpServer(IModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
            BodyDecoder = new CompositeIMessageSerializer();

            _configuration = new ChannelTcpListenerConfiguration(() => new HttpMessageDecoder(BodyDecoder), () => new HttpMessageEncoder());
            _bufferSlicePool = new BufferSlicePool(65535, 100);
            ApplicationInfo = new MemoryItemStorage();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpServer" /> class.
        /// </summary>
        /// <param name="moduleManager">The modules are used to process the HTTP requests. You need to specify at least one.</param>
        /// <param name="configuration">
        ///     You can override the configuration to your likings.
        /// </param>
        /// <exception cref="System.ArgumentNullException">moduleManager/configuration</exception>
        public HttpServer(IModuleManager moduleManager, ChannelTcpListenerConfiguration configuration)
        {
            if (moduleManager == null) throw new ArgumentNullException("moduleManager");
            if (configuration == null) throw new ArgumentNullException("configuration");

            BodyDecoder = new CompositeIMessageSerializer();
            _moduleManager = moduleManager;
            _configuration = configuration;
            _bufferSlicePool = new BufferSlicePool(65535, 100);
            ApplicationInfo = new MemoryItemStorage();
        }

        /// <summary>
        /// if set, the body decoder will be used and the server will produce <see cref="HttpRequest"/> instead of <see cref="HttpRequestBase"/>.
        /// </summary>
        /// <remarks>
        /// <para>Specified per default</para>
        /// </remarks>
        public IMessageSerializer BodyDecoder { get; set; }

        /// <summary>
        ///     You can fill this item with application specific information
        /// </summary>
        /// <remarks>
        ///     It will be supplied for every request in the <see cref="IHttpContext" />.
        /// </remarks>
        public IItemStorage ApplicationInfo { get; set; }

        /// <summary>
        /// Gets port that the server is accepting connections on
        /// </summary>
        /// <value>
        /// The local port.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Listener must have been started first.</exception>
        public int LocalPort
        {
            get
            {
                if (_listener == null)
                    throw new InvalidOperationException("Listener must have been started first.");
                return _listener.LocalPort;
            }
        }

    /// <summary>
        ///     Add a HTTP module
        /// </summary>
        /// <param name="module">Module to include</param>
        /// <remarks>Modules are executed in the order they are added.</remarks>
        public void Add(IHttpModule module)
        {
            _moduleManager.Add(module);
        }

        /// <summary>
        /// Start the HTTP server
        /// </summary>
        /// <param name="ipAddress">Address to listen on</param>
        /// <param name="port">Port to listen on.</param>
        /// <exception cref="System.ArgumentNullException">ipAddress</exception>
        /// <exception cref="System.InvalidOperationException">Stop the server before restarting.</exception>
        public void Start(IPAddress ipAddress, int port)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            if (_listener != null)
                throw new InvalidOperationException("Stop the server before restarting.");

            _listener = new HttpListener(_configuration);
            _listener.BodyDecoder = BodyDecoder;
            _listener.MessageReceived = OnClientRequest;
            _listener.Start(ipAddress, port);
        }

        /// <summary>
        ///     Start the HTTP server
        /// </summary>
        /// <param name="ipAddress">Address to listen on</param>
        /// <param name="port">Port to listen on.</param>
        /// <example>
        /// <p>You can load a certificate by doing the following:</p>
        /// <code>
        /// var certificate = new X509Certificate2(@"C:\certificates\yourCertificate", "yourpassword");
        /// var server = new HttpServer(new ModuleManager());
        /// server.Start(IPAddress.Any, 80, certifiate);
        /// </code>
        /// </example>
        public void Start(IPAddress ipAddress, int port, X509Certificate certifiate)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            if (_listener != null)
                throw new InvalidOperationException("Stop the server before restarting.");

            var factory=new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(certifiate));
            _listener = new HttpListener();
            _listener.ChannelFactory = factory;
            _listener.MessageReceived = OnClientRequest;
            _listener.Start(ipAddress, port);
        }

        private void OnClientRequest(ITcpChannel channel, object message)
        {
            var context = new HttpContext
            {
                Application = ApplicationInfo,
                Channel = channel,
                Items = new MemoryItemStorage(),
                Request = (IHttpRequest) message,
                Response = ((IHttpRequest) message).CreateResponse()
            };

            context.Response.AddHeader("X-Powered-By",
                                       "Griffin.Framework (http://github.com/jgauffin/griffin.framework)");

            _moduleManager.InvokeAsync(context, SendResponse);
        }

        private void SendResponse(IAsyncModuleResult obj)
        {
            var context = (HttpContext) obj.Context;
            context.Channel.Send(obj.Context.Response);
        }

        /// <summary>
        ///     Stop the server.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _listener = null;
        }
    }
}