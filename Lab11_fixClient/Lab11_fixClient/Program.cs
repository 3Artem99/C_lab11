using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        async public static Task Main(string[] args)
        {
            Console.WriteLine("Enter ticker: ");
            var input = Console.ReadLine();

            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1", 8888);

            var stream = tcpClient.GetStream();
            try
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(input).AsMemory());
                byte[] response = new byte[1024];
                await stream.ReadAsync(response);
                var price = Encoding.UTF8.GetString(response);
                Console.WriteLine($"Price for {input} is {price}");
            }
            catch
            {
                Console.WriteLine("smth wrong");
            }
        }
    }
}
