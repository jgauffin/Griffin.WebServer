using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Griffin.WebServer;
using Griffin.WebServer.Files;

namespace DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Module manager handles all modules in the server
            var moduleManager = new ModuleManager();

            // Let's serve our downloaded files (Windows 7 users)
            var fileService = new DiskFileService("/", string.Format(@"C:\Users\{0}\Downloads", Environment.UserName));

            // Create the file module and allow files to be listed.
            var module = new FileModule(fileService) {ListFiles = true};

            // Add the module
            moduleManager.Add(module);
            
            // And start the server.
            var server = new HttpServer(moduleManager);
            server.Start(IPAddress.Any, 8080);

            Console.ReadLine();
        }
    }
}
