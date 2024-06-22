using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

class Program
{
    class ChatClient : TcpClient
    {
        public bool IsDead { get; set; }
        protected override void Dispose(bool disposing)
        {
            IsDead = true;
            base.Dispose(disposing);
        }
    }

    static ChatClient client;

    static void Main(string[] args)
    {
        Console.WriteLine("Enter name:");
        string name = Console.ReadLine();
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        int port = 5000;
        client = new ChatClient();
        client.Connect(ip, port);
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(Console_ProcessExit);
        Console.WriteLine("Connected to the server!");
        NetworkStream ns = client.GetStream();
        byte[] name_buffer = Encoding.ASCII.GetBytes(name);
        ns.Write(name_buffer, 0, name_buffer.Length);
        Thread thread = new Thread(o => ReceiveData((ChatClient)o));

        thread.Start(client);

        string s;
        while (!string.IsNullOrEmpty((s = Console.ReadLine())))
        {
            byte[] buffer = Encoding.ASCII.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
        }

        client.Client.Shutdown(SocketShutdown.Send);
        thread.Join();
        ns.Close();
        client.Close();
        Console.WriteLine("Disconnected from server!");
    }

    static void Console_ProcessExit(object sender, EventArgs e)
    {
        if (!client.IsDead)
        {
            client.GetStream().Close();
            client.Close();
        }
    }

    static void ReceiveData(ChatClient client)
    {
        NetworkStream ns = client.GetStream();
        byte[] receivedBytes = new byte[1024];
        int byteCount;

        while ((byteCount = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
        {
            Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byteCount));
        }
    }
}