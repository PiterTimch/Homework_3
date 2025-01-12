using Microsoft.AspNetCore.SignalR.Client;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _hubConnection;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSignalR();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5135/chathub")
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatMessages.Text += $"{user}: {message}\n";
                });
            });

            try
            {
                await _hubConnection.StartAsync();
                ChatMessages.Text += "Connected to the chat server.\n";
            }
            catch (Exception ex)
            {
                ChatMessages.Text += $"Connection failed: {ex.Message}\n";
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                string user = "User";
                string message = MessageInput.Text;

                if (!string.IsNullOrEmpty(message))
                {
                    await _hubConnection.InvokeAsync("SendMessage", user, message);
                    MessageInput.Clear();
                }
            }
            else
            {
                ChatMessages.Text += "Not connected to the server.\n";
            }
        }
    }
}