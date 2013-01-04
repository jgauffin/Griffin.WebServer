using System;
using System.IO;
using System.Linq;
using Griffin.WebServer;
using Griffin.WebServer.Modules;

namespace DemoServer
{
    public class MyModule : IWorkerModule
    {
        public void BeginRequest(IHttpContext context)
        {
            
        }

        public void EndRequest(IHttpContext context)
        {
            
        }

        public void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback)
        {
            // Since this module only supports sync
            callback(new AsyncModuleResult(context, HandleRequest(context)));
        }

        public ModuleResult HandleRequest(IHttpContext context)
        {
            if (context.Request.Form.Count > 0)
                Console.WriteLine(context.Request.Form.First().Value);

            context.Response.Body = new MemoryStream();

            var writer = new StreamWriter(context.Response.Body);
                writer.WriteLine("No man");
            context.Response.Body.Position = 0;

            return ModuleResult.Continue;
        }
    }
}