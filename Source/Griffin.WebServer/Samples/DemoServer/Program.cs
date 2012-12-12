using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Griffin.Networking.Logging;
using Griffin.Networking.Protocol.Http.Services.BodyDecoders;
using Griffin.WebServer;
using Griffin.WebServer.Files;
using Griffin.WebServer.Modules;

namespace DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Assign(new SimpleLogManager<ConsoleLogger>());

            // Module manager handles all modules in the server
            var moduleManager = new ModuleManager();

            // Let's serve our downloaded files (Windows 7 users)
            var fileService = new DiskFileService("/", string.Format(@"C:\Users\{0}\Downloads", Environment.UserName));

            // Create the file module and allow files to be listed.
            var module = new FileModule(fileService) {ListFiles = true};

            // Add the module
            moduleManager.Add(module);
            moduleManager.Add(new BodyDecodingModule(new UrlFormattedDecoder()));
            moduleManager.Add(new MyModule());

            // And start the server.
            var server = new HttpServer(moduleManager);
            server.Start(IPAddress.Any, 8080);

            Console.ReadLine();
        }
    }
}
