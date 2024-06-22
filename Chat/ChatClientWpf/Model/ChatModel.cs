using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Runtime;
using System.Collections.ObjectModel;
using System.Windows;


namespace ChatClientWpf.Model
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

    public class MessagesEventArgs : EventArgs
    {
        ObservableCollection<string> messages = new ObservableCollection<string>();
        public ObservableCollection<string> Messages
        {
            get { return messages; }
        }
        public MessagesEventArgs(ObservableCollection<string> messages)
        {
            this.messages = messages;
        }
    }

    public class ChatModel
    {
        static ObservableCollection<string> messages = new ObservableCollection<string>();
        public static ObservableCollection<string> Messages
        {
            get { return messages; }
            set { messages = value; }
        }
        static ChatClient client;
        static Thread thread;
        int port = 5000;
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        public static event EventHandler<MessagesEventArgs> MessageReceived;

        public bool SetName(string name)
        {
            if(name == null || name.Length == 0)
            {
                MessageBox.Show("Name can't be empty");
                return false;
            }
            client = new ChatClient();
            client.Connect(ip, port);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(WPF_ProcessExit);
            Console.WriteLine("Connected to the server!");
            NetworkStream ns = client.GetStream();
            byte[] name_buffer = Encoding.ASCII.GetBytes(name);
            ns.Write(name_buffer, 0, name_buffer.Length);
            
            thread = new Thread(o => ReceiveData((ChatClient)o));
            thread.Start(client);
            return true;
        }

        public void Send(string message)
        {
            if(message == null || message.Length == 0)
            {
                MessageBox.Show("Message can't be empty");
                return;
            }
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        static void ReceiveData(ChatClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byteCount;

            while (!client.IsDead)
            {
                byteCount = ns.Read(receivedBytes, 0, receivedBytes.Length);
                App.Current.Dispatcher.Invoke(
                    (Action)delegate 
                    { 
                        Messages.Add(Encoding.ASCII.GetString(receivedBytes, 0, byteCount).TrimEnd()); 
                    }
                );
                MessageReceived(typeof(ChatModel), new MessagesEventArgs(Messages));
            }
        }

        static void WPF_ProcessExit(object sender, EventArgs e)
        {
            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            if (!client.IsDead)
            {
                client.GetStream().Close();
                client.Close();
            }
        }
    }
}