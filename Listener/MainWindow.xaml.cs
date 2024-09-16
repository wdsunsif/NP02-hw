using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace Listener
{
    public partial class MainWindow : Window
    {
        private Socket client;
        private EndPoint remoteEP;

        public MainWindow()
        {
            InitializeComponent();

            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var ip = IPAddress.Parse("127.0.0.1");
            var port = 45678;
            remoteEP = new IPEndPoint(ip, port);
        }

        private static BitmapImage? LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var buffer = new byte[ushort.MaxValue - 29];
            client.SendTo(buffer, remoteEP);
            var list = new List<byte>();
            var len = 0;
            var sumBytes = 0;

            do
            {
                await Task.Delay(50);
                var result = await client.ReceiveFromAsync(buffer, SocketFlags.None, remoteEP);

                len = result.ReceivedBytes;

                list.AddRange(buffer.Take(len));

                sumBytes += len;
            } while (len == buffer.Length);


            try
            {
                var image = LoadImage(list.ToArray());
                Img.Source = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
