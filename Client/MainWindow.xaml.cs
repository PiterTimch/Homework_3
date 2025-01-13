using Client.Connecting;
using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualBasic.ApplicationServices;
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

            _serverInChat = new UserView() // доробити
            { 
                Name = "Server"
            };
        }

        private async void InitializeSignalR(string serverURL)
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{serverURL}/chathub")
                .Build();

                _hubConnection.On<string, string>("ReceiveMessage", (userName, message) =>
                {
                    Dispatcher.Invoke(async () =>
                    {
                        UserView anotherUser = new UserView() { Name = userName }; //доробити
                        AppendTextToRichTextBox(anotherUser, message);
                    });
                });

                await _hubConnection.StartAsync();
                AppendTextToRichTextBox(_serverInChat, "Connected to the chat server");
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox(_serverInChat, $"Connection failed: {ex.Message}");
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

                if (sendBT.Content.ToString() == _sendBtText)
                {
                    if (_hubConnection.State == HubConnectionState.Connected)
                    {
                        string message = this.messageInputTB.Text;

                        if (!string.IsNullOrEmpty(message))
                        {
                            await _hubConnection.InvokeAsync("SendMessage", _user.Name, message);
                        }
                    }
                    else
                    {
                        AppendTextToRichTextBox(_serverInChat, "Not connected to the server.");
                    }
                }
                else
                {
                    await SendImage(_user, this.messageInputTB.Text);
                }

                this.sendBT.Content = _sendBtText;
                this.messageInputTB.Clear();
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox(_serverInChat, ex.Message);
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

        private void AppendTextToRichTextBox(UserView user, string message) //доробити
        {
            Run userText = new Run($"{user.Name}: ") { Foreground = System.Windows.Media.Brushes.Blue };
            Run messageText = new Run($"{message}\n");

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(userText);
            paragraph.Inlines.Add(messageText);

            this.chatMessagesTB.Document.Blocks.Add(paragraph);

            this.chatMessagesTB.ScrollToEnd();
        }

        private async Task SendImage(UserView user, string imagePath)//доробити
        {
            Run userText = new Run($"{user.Name}: ") { Foreground = System.Windows.Media.Brushes.Blue };

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            BitmapImage bitmap = ImageSender.GetImageFromServer(
                await ImageSender.UploadImageAsync(imagePath, _imageServer),
                _imageServer
            );

            double originalWidth = bitmap.PixelWidth;
            double originalHeight = bitmap.PixelHeight;
            double targetHeight = 100;
            double aspectRatio = originalWidth / originalHeight;

            image.Height = targetHeight;
            image.Width = targetHeight * aspectRatio;
            image.Source = bitmap;

            InlineUIContainer container = new InlineUIContainer(image);
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(userText);
            paragraph.Inlines.Add(container);

            this.chatMessagesTB.Document.Blocks.Add(paragraph);
        }
    }
}