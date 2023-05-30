using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using AdvancedCommands;
using OnlineStore.Core;

namespace OnlineStore.CLI;

public static class Program
{
    public static CommandRunner Runner = new();
    public static void Main(string[] args)
    {
        DoClient();
    }

    public static void DoClient()
    {
        try
        {
            Task.Run(async () =>
            {
                IPEndPoint ip = new(IPAddress.Parse("127.0.0.1"), 1030);
            
                ClientResponse? response = await Backend.ClientInit(ip, "firebreak", "1234");

                if (response is null)
                {
                    Console.WriteLine("No host found");
                    return;
                }

                int connectionId = int.Parse(response.Message!);
            
                // Console.WriteLine(await Backend.ClientSend(ip, "ADDCRED;5;firebreak"));
                Console.WriteLine(await Backend.ClientSend(ip, $"BUY;{connectionId};0,3,2"));
            
            }).GetAwaiter().GetResult();
        }
        catch (SocketException e)
        {
            Console.WriteLine("Host disconnect");
            return;
        }
    }
}