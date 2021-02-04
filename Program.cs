using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playground
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Starting... {Environment.MachineName}");
            
            using (var wsClient = new ClientWebSocket())
            {
                await wsClient.ConnectAsync(new Uri("ws://host.docker.internal:8080"), new CancellationToken());

                var encoded = Encoding.UTF8.GetBytes(Environment.MachineName);
                var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await wsClient.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationToken());
            }

            Console.Write(" Done");
            Thread.Sleep(Timeout.Infinite);
            Console.Write(" Sera?");
        }
    }
}
