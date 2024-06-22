using ChatClientWpf.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ChatClientWpf.ViewModel
{
    public partial class ChatViewModel : ObservableObject, INotifyPropertyChanged
    {
        public ChatModel chatModel;
        public ChatViewModel(ChatModel chatModel)
        {
            this.chatModel = chatModel;
            ChatModel.MessageReceived += new EventHandler<MessagesEventArgs>(Model_MessageReceived);
        }

        private void Model_MessageReceived(object? sender, MessagesEventArgs e)
        {
            Messages = e.Messages;
        }

        [ObservableProperty]
        ObservableCollection<string> messages = new ObservableCollection<string>();
        
        [ObservableProperty]
        string message = "";

        [ObservableProperty]
        string user = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotConnected))]
        bool isConnected;

        public bool IsNotConnected => !IsConnected;

        [RelayCommand]
        void Send()
        {
            chatModel.Send(Message);
        }

        [RelayCommand]
        void Connect()
        {
            bool state = chatModel.SetName(User);
            if (state)
                IsConnected = true;
        }
    }
}
