using Client.Connecting;
using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private HttpClient _imageServer;

        public StartWindow()
        {
            InitializeComponent();
            InitializeHttpClient();
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
                System.Windows.MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void joinBT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsValidInput())
                {
                    throw new Exception("There are empty input fields");
                }

                UserView user = new UserView();

                user.Name = this.userNameTB.Text;
                user.AvatarImgPath = await ImageSender.UploadImageAsync(this.avatarPathTB.Text, _imageServer);

                new MainWindow(user, this.serverURLTB.Text, _imageServer).Show();
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidInput()
        {
            return !String.IsNullOrEmpty(this.serverURLTB.Text) && !String.IsNullOrWhiteSpace(this.serverURLTB.Text) &&
                !String.IsNullOrEmpty(this.userNameTB.Text) && !String.IsNullOrWhiteSpace(this.userNameTB.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.avatarPathTB.Text = openFileDialog.FileName;
                }
            }
        }
    }
}
