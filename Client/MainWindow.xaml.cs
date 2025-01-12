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
        private string _userName;

        public MainWindow(string userName, string serverURL)
        {
            InitializeComponent();
            InitializeSignalR(serverURL);
            _userName = userName;
        }

        private async void InitializeSignalR(string serverURL)
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{serverURL}/chathub")
                .Build();

                _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.chatMessagesTB.Text += $"{user}: {message}\n";
                    });
                });

                await _hubConnection.StartAsync();
                this.chatMessagesTB.Text += "Connected to the chat server.\n";
            }
            catch (Exception ex)
            {
                this.chatMessagesTB.Text += $"Connection failed: {ex.Message}\n";
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hubConnection == null) 
                {
                    throw new Exception("Invalid server");
                }

                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    string message = this.messageInputTB.Text;

                    if (!string.IsNullOrEmpty(message))
                    {
                        await _hubConnection.InvokeAsync("SendMessage", _userName, message);
                        this.messageInputTB.Clear();
                    }
                }
                else
                {
                    this.chatMessagesTB.Text += "Not connected to the server.\n";
                }
            }
            catch (Exception ex)
            {
                this.chatMessagesTB.Text += ex.Message + '\n';
            }
        }

        private void reloginBT_Click(object sender, RoutedEventArgs e)
        {
            new StartWindow().Show();
            this.Close();
        }
    }
}