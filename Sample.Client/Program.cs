using Sample.Data;
using System;
using System.Linq;

namespace Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press key when Server is ready...");
            Console.ReadLine();
            var ws = new WebsocketQueryRemoteHandler("ws://127.0.0.1/ws");
            var ctx = new QueryableProvider(ws);
            var qry = ctx.Query<Dto1>().Where(x => x.Name.Contains("aa")).ToList();
        }
    }
}
