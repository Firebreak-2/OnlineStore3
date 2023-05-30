using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OnlineStore.Core;

public static class Backend
{
    public static bool Running = false;
    public static List<ClientConnection> ClientConnections = new();

    public static async Task SafeHostInit(int port)
    {
        HostStart:
        try
        {
            await HostInit(port);
        }
        catch (Exception e)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            Console.ResetColor();

            if (Running)
                goto HostStart;

            throw;
        }
    }

    public static async Task<ClientResponse?> ClientInit(IPEndPoint ip, string username, string password)
    {
        ClientResponse? response = await ClientSend(ip, $"CONN;{username};{password}");

        return response;
    }

    public static async Task<ClientResponse?> ClientSend(IPEndPoint ip, string message)
    {
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await socket.ConnectAsync(ip); //Connect to the server
        }
        catch (SocketException _)
        {
            Console.WriteLine("No host found");
            return null;
        }

        socket.Send(Encoding.ASCII.GetBytes(message)); //Encode from message, send the data

        byte[] data = new byte[1024];
        int receivedDataLength = socket.Receive(data); //Wait for the data
        string[] stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength).Split(';'); //Decode the data received

        ClientResponseStatus responseStatus = Enum.Parse<ClientResponseStatus>(stringData[0]);
        string? responseMessage = stringData.Length == 1 ? null : stringData[1];

        ClientResponse response = new(responseStatus, responseMessage);

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        
        return response;
    }

    public static async Task HostInit(int port)
    {
        Running = true;
        Shop.EnsureDatabase();

        IPEndPoint ip = new(IPAddress.Any, port); //Any IPAddress that connects to the server on any port
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket

        socket.Bind(ip); //Bind to the client's IP

        Console.WriteLine("-- Running --");
        while (Running)
        {
            socket.Listen(1); //Listen for a     connection
            Socket client = socket.Accept();

            IPEndPoint? clientep = (IPEndPoint?) client.RemoteEndPoint;

            byte[] data = new byte[1024];
            int receivedDataLength = await client.ReceiveAsync(data); //Wait for the data
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength); //Decode the data received

            if (string.IsNullOrEmpty(stringData))
                break;

            Console.WriteLine($"[{clientep?.Address}:{clientep?.Port}] > {stringData}");

            switch (stringData.Split(';'))
            {
                case ["CONN", string username, string password]:
                    string token = username + password;

                    ClientConnection connection = new(token, username, DateTime.Now);
                    ClientConnections.Add(connection);

                    await client.SendAsync(Encoding.UTF8.GetBytes($"OK;{ClientConnections.Count - 1}"), SocketFlags.None);
                    break;

                case ["ADDCRED", string creditAmountString, string userByName]:
                    int creditAmount = int.Parse(creditAmountString);

                    Shop.LoadDatabase();
                    string accountToken = ClientConnections.First(x => string.Equals(x.Username, userByName, StringComparison.InvariantCultureIgnoreCase)).Token;
                    Shop.AccountBalances.TryAdd(accountToken, 0);
                    Shop.AccountBalances[accountToken] += creditAmount;
                    Shop.SaveDatabase();

                    await client.SendAsync("OK"u8.ToArray(), SocketFlags.None);
                    break;

                case ["BUY", string connectionIdString, string itemsToPurchaseString]:
                    int connectionId = int.Parse(connectionIdString);
                    int[] itemsToPurchase = itemsToPurchaseString.Split(',').Select(int.Parse).ToArray();
                    
                    Shop.LoadDatabase();

                    int itemsCost = itemsToPurchase.Sum(x => Shop.Items[x].Cost);

                    string buyerToken = ClientConnections[connectionId].Token;
                    Shop.AccountBalances.TryAdd(buyerToken, 0);
                    int balance = Shop.AccountBalances[buyerToken];
                    if (itemsCost > balance)
                    {
                        await client.SendAsync(Encoding.UTF8.GetBytes($"OKERR;Insufficient funds (cost [{itemsCost}] > balance [{balance}])"), SocketFlags.None);
                        break;
                    }

                    Shop.AccountBalances[buyerToken] -= itemsCost;

                    Console.WriteLine($"[{connectionId}] BUYS {string.Join(", ", itemsToPurchase.Select(x => Shop.Items[x].Name))}");
                    await client.SendAsync("OK"u8.ToArray(), SocketFlags.None);
                    Shop.SaveDatabase();
                    break;
            }

            client.Close();
        }

        Console.WriteLine("-- End --");
    }
}