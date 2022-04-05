using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DreamingPhoenix.Extensions
{
    public static class ImageOperationExtensions
    {
        private static BitmapImage LoadImageFromBytes(byte[] imageData)
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

        public static async Task<ImageSource> LoadImage(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;

            var webClient = new WebClient();
            try
            {
                byte[] imageBytes = await webClient.DownloadDataTaskAsync(new Uri(uri));
                return LoadImageFromBytes(imageBytes);
            }
            catch
            {
                return null;
            }
        }
    }
}
