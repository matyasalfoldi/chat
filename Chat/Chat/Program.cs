using System.Net.Sockets;
using System.Net;
using System.Text;
using static Program;

class Program
{
    public struct User
    {
        public TcpClient client;
        public string name;
        public User(TcpClient _client, string _name = "")
        {
            client = _client;
            name = _name;
        }
    }

    static readonly object _lock = new object();
    static readonly Dictionary<int, User> list_users = new Dictionary<int, User>();

    static void Main(string[] args)
    {
        int count = 1;

        TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
        ServerSocket.Start();

        while (true)
        {
            TcpClient client = ServerSocket.AcceptTcpClient();
            User user = new User(client);
            lock (_lock)
            {
                user.name = GetUsername(user.client);
                list_users.Add(count, user);
            }
            Console.WriteLine($"{user.name} connected");

            Thread t = new Thread(HandleClients);
            t.Start(count++);
        }
    }

    public static string GetUsername(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int byte_count = stream.Read(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, byte_count);
    }

    public static void HandleClients(object o)
    {
        int id = (int)o;
        User user;

        lock (_lock) user = list_users[id];

        while (true)
        {
            NetworkStream stream = user.client.GetStream();
            byte[] buffer = new byte[1024];
            Console.WriteLine($"{user.name}:"+Encoding.ASCII.GetString(buffer, 0, buffer.Length).Trim());
            int byte_count = stream.Read(buffer, 0, buffer.Length);

            if (byte_count == 0)
            {
                break;
            }

            string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
            Broadcast(data, user.name);
            Console.WriteLine(data);
        }

        lock (_lock) list_users.Remove(id);
        user.client.Client.Shutdown(SocketShutdown.Both);
        user.client.Close();
        Console.WriteLine($"{user.name} connection closed");
    }

    public static void Broadcast(string data, string user)
    {
        byte[] buffer = Encoding.ASCII.GetBytes($"{user}:");
        buffer = buffer.Concat(Encoding.ASCII.GetBytes(data + Environment.NewLine)).ToArray();
        lock (_lock)
        {
            foreach (User c in list_users.Values)
            {
                try
                {
                    NetworkStream stream = c.client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch( Exception e )
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}