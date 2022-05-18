using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Start_Robot_2._1
{
    class ImageConverter
    {
        /// <summary>
        /// Creates BitmapImage source from bytes array. Must be called in UI dispatcher.
        /// </summary>
        /// <param name="array">JPEG Image byte array</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> GetBitmapAsync(byte[] array, int width = 640 / 4, int height = 480 / 4)
        {
            var bitmapImage = new WriteableBitmap(width, height);

            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(stream))
                {
                    writer.WriteBytes(array);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                    writer.DetachStream();
                }

                stream.Seek(0);
                await bitmapImage.SetSourceAsync(stream);
            }

            return bitmapImage;
        }
    }
}
