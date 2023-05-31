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
        Buy();
        return;
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

                ClientResponse? buyResponse = await Backend.ClientSend(ip, $"BUY;{connectionId};{Buy()}");
                if (buyResponse?.Status == ClientResponseStatus.OKERR)
                {
                    Console.Clear();
                    Console.WriteLine("Insufficient Funds");
                    return;
                }

            }).GetAwaiter().GetResult();
        }
        catch (SocketException e)
        {
            Console.WriteLine("Host disconnect");
            return;
        }
    }

    public static string Buy()
    {
        int selectedItemIndex = 0;
        List<int> thingsToBuy = new();
        while (true)
        {
            Console.Clear();
            for (var i = 0; i < Shop.Items.Length; i++)
            {
                ShopItem item = Shop.Items[i];
            
                Console.WriteLine($"[{(selectedItemIndex == i ? "*" : " ")}] {item.Name} - {item.Cost}AED");
            }

            Dictionary<int, int> compactThingsToBuy = new();
            int totalCost = 0;

            foreach (int itemId in thingsToBuy)
            {
                compactThingsToBuy.TryAdd(itemId, 0);
                
                compactThingsToBuy[itemId]++;
                totalCost += Shop.Items[itemId].Cost;
            }

            Console.WriteLine();
            Console.WriteLine(string.Join('\n', compactThingsToBuy.Select(x => $"* {Shop.Items[x.Key].Name} x{x.Value}")));

            Console.WriteLine();
            Console.WriteLine($"Total Cost: {totalCost}");

            Console.WriteLine();
            Console.WriteLine("-> Add     <- Remove     ENTER Confirm");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.UpArrow:
                    selectedItemIndex = selectedItemIndex == 0 ? Shop.Items.Length - 1 : selectedItemIndex - 1;
                    break;
                case ConsoleKey.DownArrow:
                    selectedItemIndex = selectedItemIndex == Shop.Items.Length - 1 ? 0 : selectedItemIndex + 1;
                    break;
                case ConsoleKey.RightArrow:
                    thingsToBuy.Add(selectedItemIndex);
                    break;
                case ConsoleKey.LeftArrow:
                    if (thingsToBuy.Contains(selectedItemIndex))
                        thingsToBuy.Remove(selectedItemIndex);
                    break;
                case ConsoleKey.Enter:
                    return string.Join(',', thingsToBuy);
            }
        }
    }
}