using Client.Connecting;
using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualBasic.ApplicationServices;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        private HttpClient _imageServer;
        
        private UserView _user;
        private UserView _serverInChat;

        private string _sendBtText = "(っ'-')╮==💌";
        private string _sendBtImage = "(っ'-')╮==🌄";
        public MainWindow(UserView user, string serverURL, HttpClient imageServer)
        {
            InitializeComponent();
            InitializeSignalR(serverURL);
            _imageServer = imageServer;
            _user = user;

            _serverInChat = new UserView()
            { 
                Name = "Server",
                AvatarImgPath = "/images/68db153d-3e09-46ab-ad57-015ccdf10661.jpg"
            };
        }

        private async void InitializeSignalR(string serverURL)
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{serverURL}/chathub")
                .Build();

                _hubConnection.On<string, string, string>("ReceiveMessage", (userName, message, imagePath) =>
                {
                    Dispatcher.Invoke(async () =>
                    {
                        UserView anotherUser = new UserView() { Name = userName, AvatarImgPath = imagePath };
                        if (message.Contains("/") || message.Contains("\\"))
                        {
                            await AppendImageMessage(anotherUser, message);
                        }
                        else
                        {
                            AppendTextMessage(anotherUser, message);
                        }
                    });
                });

                await _hubConnection.StartAsync();
                AppendTextMessage(_serverInChat, "Connected to the chat server");
            }
            catch (Exception ex)
            {
                AppendTextMessage(_serverInChat, $"Connection failed: {ex.Message}");
            }
        }

        private async void sendBT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hubConnection == null) 
                {
                    throw new Exception("Invalid server");
                }

                string message = this.messageInputTB.Text;

                if (sendBT.Content.ToString() == _sendBtText)
                {
                    if (_hubConnection.State == HubConnectionState.Connected)
                    {

                        if (!string.IsNullOrEmpty(message))
                        {
                            await _hubConnection.InvokeAsync("SendMessage", _user.Name, message, _user.AvatarImgPath);
                        }
                    }
                    else
                    {
                        AppendTextMessage(_serverInChat, "Not connected to the server.");
                    }
                }
                else
                {
                    await _hubConnection.InvokeAsync("SendMessage", _user.Name, message, _user.AvatarImgPath);
                }

                this.sendBT.Content = _sendBtText;
                this.messageInputTB.Clear();
            }
            catch (Exception ex)
            {
                AppendTextMessage(_serverInChat, ex.Message);
            }
        }

        private void reloginBT_Click(object sender, RoutedEventArgs e)
        {
            new StartWindow().Show();
            this.Close();
        }

        private void selectImageBT_Click(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.messageInputTB.Text = openFileDialog.FileName;
                    this.sendBT.Content = _sendBtImage;
                }
            }
        }

        private void AppendTextMessage(UserView user, string message)
        {
            this.chatMessagesTB.Document.Blocks.Add(ClientMessageSender.CreateMessageText(user, message, _imageServer));

            this.chatMessagesTB.ScrollToEnd();
        }

        private async Task AppendImageMessage(UserView user, string imagePath)
        {
            this.chatMessagesTB.Document.Blocks.Add(await ClientMessageSender.CreateMessageImage(user, imagePath, _imageServer));
            
            this.chatMessagesTB.ScrollToEnd();
        }
    }
}