using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Griffin.Logging;
using Griffin.Logging.Loggers;
using Griffin.WebServer;
using Griffin.WebServer.Files;
using Griffin.WebServer.Modules;

namespace DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //LogManager.Assign(new SimpleLogManager<ConsoleLogger>());

            // Module manager handles all modules in the server
            var moduleManager = new ModuleManager();

            // Let's serve our downloaded files (Windows 7 users)
            var fileService = new DiskFileService("/", string.Format(@"C:\Users\{0}\Downloads", Environment.UserName));

            // Create the file module and allow files to be listed.
            var module = new FileModule(fileService) {AllowFileListing = true};

            // Add the module
            moduleManager.Add(module);
            moduleManager.Add(new MyModule());

            moduleManager.Add(new MyModule2());
            // And start the server.
            var server = new HttpServer(moduleManager);
            server.Start(IPAddress.Any, 0);
            Console.WriteLine("PORT " + server.LocalPort);

            //TrySendARequest(server);

            Console.ReadLine();
        }

        private static void TrySendARequest(HttpServer server)
        {
            var request =
                @"GET /?signature=1dfea26808d632903549c69d78558fce1c418405&echostr=5867553698596935317&timestamp=1365661332&nonce=1366146317 HTTP/1.0
User-Agent:Mozilla/4.0
Host:58.215.164.183
Pragma:no-cache
Connection/Value:Keep-Alive

";
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, server.LocalPort);
            socket.Send(Encoding.UTF8.GetBytes(request));
            var buffer = new byte[65535];
            var len = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            var answer = Encoding.UTF8.GetString(buffer, 0, len);
            Console.WriteLine(answer);
            len = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            answer = Encoding.UTF8.GetString(buffer, 0, len);
            Console.WriteLine(answer);
        }
    }
}
