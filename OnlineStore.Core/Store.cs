using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OnlineStore.Core;

public static class Store
{
    public static void Initialize(bool host, int port)
    {
        if (host)
            HostInit(port);
        else
            ClientInit(port);
    }

    public static void ClientInit(int port)
    {
        IPEndPoint ip = new(IPAddress.Parse("127.0.0.1"), port);

        Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Connect(ip); //Connect to the server
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

    private static void HostInit(int port)
    {
        IPEndPoint ip = new(IPAddress.Any, port); //Any IPAddress that connects to the server on any port
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket

        socket.Bind(ip); //Bind to the client's IP
        socket.Listen(10); //Listen for maximum 10 connections
        Console.WriteLine("Waiting for a client...");
        Socket client = socket.Accept();
        IPEndPoint? clientep = (IPEndPoint?) client.RemoteEndPoint;

        Console.WriteLine($"Connected with {clientep?.Address} at port {clientep?.Port}");

        const string responseData = "Welcome";
        byte[] sendData = Encoding.ASCII.GetBytes(responseData);
        client.Send(sendData, sendData.Length, SocketFlags.None);
        
        while (true)
        {
            byte[] data = new byte[1024];
            int receivedDataLength = client.Receive(data); //Wait for the data
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength); //Decode the data received
            Console.WriteLine(stringData); //Write the data on the screen
        }
        
        Console.WriteLine($"Disconnected from {clientep?.Address}");
        client.Close();
        socket.Close();
    }
}