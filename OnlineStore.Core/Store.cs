using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OnlineStore.Core;

public static class Store
{
    public static bool Running = false;
    public static List<Socket> ClientList = new();

    public static async Task Initialize(bool host, int port)
    {
        Running = true;

        if (host) await HostInit(port);
        else await ClientInit(port);
    }

    private static async Task ClientInit(int port)
    {
        IPEndPoint ip = new(IPAddress.Parse("127.0.0.1"), port);

        Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await server.ConnectAsync(ip); //Connect to the server
        }
        catch (SocketException _)
        {
            Console.WriteLine("Unable to connect to server.");
            return;
        }

        byte[] data = new byte[1024];
        int receivedDataLength = server.Receive(data); //Wait for the data
        string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength); //Decode the data received
        Console.WriteLine($"Recieved: {stringData}"); //Write the data on the screen

        Console.WriteLine("Send data:");
        Console.WriteLine("Type 'exit' to exit.");
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            if (input == "exit")
                break;
            server.Send(Encoding.ASCII.GetBytes(input)); //Encode from user's input, send the data
        }

        server.Shutdown(SocketShutdown.Both);
        server.Close();
    }

    private static async Task HostInit(int port)
    {
        IPEndPoint ip = new(IPAddress.Any, port); //Any IPAddress that connects to the server on any port
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket

        socket.Bind(ip); //Bind to the client's IP
        socket.Listen(10); //Listen for maximum 10 connections
        Console.WriteLine("Waiting for a client...");

        ClientList.Add(socket.Accept());

        foreach (Socket client in ClientList)
        {
            IPEndPoint? clientep = (IPEndPoint?)client.RemoteEndPoint;

            Console.WriteLine($"Connected with {clientep?.Address} at port {clientep?.Port}");

            await client.SendAsync("Welcome"u8.ToArray(), SocketFlags.None);

            while (Running)
            {
                byte[] data = new byte[1024];
                int receivedDataLength = await client.ReceiveAsync(data); //Wait for the data
                string? stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength); //Decode the data received

                if (stringData is null)
                    break;

                switch (stringData)
                {
                    case "clear":
                        {
                            Console.Clear();
                            break;
                        }
                    // i love janky code !!!!!!
                    case ['e', 'c', 'h', 'o', ' ', .. var args]:
                        {
                            Console.WriteLine(string.Join('\0', args));
                            break;
                        }
                    case "bye":
                        {
                            Running = false;
                            await client.SendAsync(data, SocketFlags.None);
                            break;
                        }
                    default:
                        {
                            Console.WriteLine($"invalid request: {stringData}");
                            break;
                        }
                }
            }

            Console.WriteLine($"Disconnected from {clientep?.Address}");
            client.Close();
            socket.Close();
        }

        ClientList.Clear();
    }
}
