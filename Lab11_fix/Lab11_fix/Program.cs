using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace Server
{
    public class Ticker
    {
        public int id { get; set; }
        public string tickername { get; set; }
    }

    public class price
    {
        public int id { get; set; }
        public int tickerid { get; set; }
        public double priceval { get; set; }
        public string date { get; set; }
    }

    public class todayscondition
    {
        public int id { get; set; }
        public int tickerid { get; set; }
        public bool state { get; set; }
    }

    public class StockContext : DbContext
    {
        public DbSet<Ticker> tickers { get; set; }
        public DbSet<price> prices { get; set; }
        public DbSet<todayscondition> todayscondition { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=MyData;Username=postgres;Password=postgres");
        }
    }

    public class Program
    {
        public async static Task Main(string[] arg)
        {
            using (var dc = new StockContext())
            {
                await Console.Out.WriteLineAsync(dc.tickers.First().tickername.ToString());
            }
            var tcpListener = new TcpListener(IPAddress.Any, 8888);

            tcpListener.Start();
            try
            {
                while (true)
                {
                    using var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    var stream = tcpClient.GetStream();
                    byte[] data = new byte[256];
                    int bytesRead = await stream.ReadAsync(data);
                    var s = Encoding.UTF8.GetString(data[..bytesRead]);
                    using (var context = new StockContext())
                    {
                        var ticker = context.tickers.FirstOrDefault(t => t.tickername == s);
                        var price = context.prices.FirstOrDefault(p => p.id == ticker.id).priceval;
                        await stream.WriteAsync(Encoding.UTF8.GetBytes(Convert.ToString(price)));
                    }
                }
            }   
            finally
            {
                tcpListener.Stop();
            }
        }
    }
}