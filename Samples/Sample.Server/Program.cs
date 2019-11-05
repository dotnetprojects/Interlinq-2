using System;
using System.Net;

namespace Sample.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server  = new WebSocketSharp.Server.WebSocketServer(IPAddress.Any, 80);
            server.AddWebSocketService("/ws", () => new SampleWebSocketServerBehavior());
            server.Start();
            Console.WriteLine("Server ready - Press Key to exit");
            Console.ReadLine();
        }
    }
}
