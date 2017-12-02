using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ImageSandbox.Utilities.Retriever
{
    public static class DotsPerInch
    {
        public static async Task<double> RetrieveX(StorageFile imageFile)
        {
            using (var embedfileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(embedfileStream);
                var x = decoder.DpiX;
                return x;
            }
        }

        public static async Task<double> RetrieveY(StorageFile imageFile)
        {
            using (var embedfileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(embedfileStream);
                var y = decoder.DpiY;
                return y;
            }
        }
    }
}