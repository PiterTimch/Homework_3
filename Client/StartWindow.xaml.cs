using System;
using System.Collections.Generic;
using System.Linq;
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
        public StartWindow()
        {
            InitializeComponent();
        }

        private void hoinBT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsValidInput())
                {
                    throw new Exception("There are empty input fields");
                }
                new MainWindow(this.userNameTB.Text, this.serverURLTB.Text).Show();
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
    }
}
