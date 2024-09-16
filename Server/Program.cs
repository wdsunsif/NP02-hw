using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.IO;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        var ip = IPAddress.Parse("127.0.0.1");
        var port = 45678;
        var buffer = new byte[ushort.MaxValue - 29];
        var listenerEP = new IPEndPoint(ip, port);
        EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        listener.Bind(listenerEP);

        Image ScreenShot()
        {
            Bitmap memoryImage;
            memoryImage = new Bitmap(1920, 1080);

            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(0, 0, 0, 0, memoryImage.Size);

            return (Image)memoryImage;
        }

        byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        while (true)
        {
            var result = await listener.ReceiveFromAsync(buffer, SocketFlags.None, ep);
            var img = ScreenShot();
            var imgBuffer = ImageToByte(img);

            Console.WriteLine(imgBuffer.Length);

            var chunk = imgBuffer.Chunk(ushort.MaxValue - 29);
            var newBuffer = chunk.ToArray();

            for (int i = 0; i < newBuffer.Length; i++)
            {
                await Task.Delay(50);
                await listener.SendToAsync(newBuffer[i], SocketFlags.None, result.RemoteEndPoint);
            }
        }


    }
}