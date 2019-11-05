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
            var qry = from d1 in ctx.Query<Dto1>()
                      join d2 in ctx.Query<Dto2>() on d1.Name equals d2.Name
                      select new { d1, d2 };

            var res = qry.ToList();
        }
    }
}
