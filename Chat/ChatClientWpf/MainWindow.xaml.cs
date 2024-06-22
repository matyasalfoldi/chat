using System.Windows;
using ChatClientWpf.ViewModel;
using ChatClientWpf.Model;

namespace ChatClientWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ChatViewModel(new ChatModel());
        }
    }
}