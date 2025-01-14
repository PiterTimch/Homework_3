using Client.Models;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Client.Connecting
{
    public static class ClientMessageSender
    {
        public static Paragraph CreateMessageText(UserView user, string message, HttpClient imageServer)
        {
            Run userText = new Run($"{user.Name}: ") { Foreground = System.Windows.Media.Brushes.Blue };
            Run messageText = new Run($"{message}\n");

            Paragraph paragraph = new Paragraph();

            paragraph.Inlines.Add(CreateImageForMessage(user.AvatarImgPath, 40, imageServer));
            paragraph.Inlines.Add(userText);
            paragraph.Inlines.Add(messageText);

            return paragraph;
        }

        public static async Task<Paragraph> CreateMessageImage(UserView user, string imagePath, HttpClient imageServer)
        {
            Run userText = new Run($"{user.Name}: ") { Foreground = System.Windows.Media.Brushes.Blue };

            Paragraph paragraph = new Paragraph();

            paragraph.Inlines.Add(CreateImageForMessage(user.AvatarImgPath, 40, imageServer));
            paragraph.Inlines.Add(userText);
            paragraph.Inlines.Add(CreateImageForMessage(await ImageSender.UploadImageAsync(imagePath, imageServer), 100, imageServer));

            return paragraph;
        }

        private static InlineUIContainer CreateImageForMessage(string serverImagePath, int height, HttpClient imageServer) 
        {
            System.Windows.Controls.Image avatarImage = new System.Windows.Controls.Image();

            BitmapImage bitmap = ImageSender.GetImageFromServer(
                serverImagePath,
                imageServer
            );

            double originalWidth = bitmap.PixelWidth;
            double originalHeight = bitmap.PixelHeight;
            double targetHeight = height;
            double aspectRatio = originalWidth / originalHeight;

            avatarImage.Height = targetHeight;
            avatarImage.Width = targetHeight * aspectRatio;
            avatarImage.Source = bitmap;
            avatarImage.Margin = new System.Windows.Thickness(5,0,5,0);

            return new InlineUIContainer(avatarImage);
        }
    }
}
