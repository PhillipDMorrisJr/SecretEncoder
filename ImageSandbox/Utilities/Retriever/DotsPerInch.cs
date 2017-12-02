using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Utilities.Retriever
{
    public static class DotsPerInch
    {
        public static async Task<double> RetrieveX(StorageFile imageFile)
        {
            double x = 0;
            using (var embedfileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(embedfileStream);
                x = decoder.DpiX;
            }
            return x;
        }
        public static async Task<double> RetrieveY(StorageFile imageFile)
        {
            double y = 0;
            using (var embedfileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(embedfileStream);
                y = decoder.DpiY;
            }
            return y;
        }
    }
}
