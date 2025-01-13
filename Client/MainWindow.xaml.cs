using Client.Connecting;
using Microsoft.AspNetCore.SignalR.Client;
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
        
        private string _userName; //зробити клас

        private string _sendBtText = "(っ'-')╮==💌";
        private string _sendBtImage = "(っ'-')╮==🌄";
        public MainWindow(string userName, string serverURL)
        {
            InitializeComponent();
            InitializeSignalR(serverURL);
            InitializeHttpClient();
            _userName = userName;
        }

        private async void InitializeHttpClient()
        {
            try
            {
                _imageServer = new HttpClient { BaseAddress = new Uri("https://kukumber.itstep.click/") };

                HttpResponseMessage response = await _imageServer.PostAsync("api/galleries/upload", null);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new Exception("Invalid port");
                }
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox("Server", $"Connection failed: {ex.Message}\n");
            }
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
                        AppendTextToRichTextBox(user, message);
                    });
                });

                await _hubConnection.StartAsync();
                AppendTextToRichTextBox("Server", "Connected to the chat server");
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox("Server", $"Connection failed: {ex.Message}");
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
                            await _hubConnection.InvokeAsync("SendMessage", _userName, message);
                        }
                    }
                    else
                    {
                        AppendTextToRichTextBox("Server", "Not connected to the server.");
                    }
                }
                else
                {
                    await SendImage(_userName, this.messageInputTB.Text);
                }

                this.sendBT.Content = _sendBtText;
                this.messageInputTB.Clear();
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox("Server", ex.Message);
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

        private void AppendTextToRichTextBox(string user, string message)
        {
            Run userText = new Run($"{user}: ") { Foreground = System.Windows.Media.Brushes.Blue };
            Run messageText = new Run($"{message}\n");

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(userText);
            paragraph.Inlines.Add(messageText);

            this.chatMessagesTB.Document.Blocks.Add(paragraph);

            this.chatMessagesTB.ScrollToEnd();
        }

        private async Task SendImage(string user, string imagePath)
        {
            Run userText = new Run($"{user}: ") { Foreground = System.Windows.Media.Brushes.Blue };

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