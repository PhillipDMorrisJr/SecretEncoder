using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Utilities.Converter;

namespace ImageSandbox.Utilities.Encryption
{
    /// <summary>
    ///     Encrypts an image into an image.
    /// </summary>
    public static class ImageEncryption
    {
        public static async Task<byte[]> EncryptAsync(Image image, StorageFile sourceImageFile)
        {
            var imageToEncrypt = WriteableBitmapConverter.ConvertToWriteableBitmap(image);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(imageToEncrypt.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(imageToEncrypt.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );


                var sourcePixels = pixelData.DetachPixelData();
                var writeableBitmap = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

                var quadrant1 = new byte[sourcePixels.Length / 4];
                var quadrant2 = new byte[sourcePixels.Length / 4];
                var quadrant3 = new byte[sourcePixels.Length / 4];
                var quadrant4 = new byte[sourcePixels.Length / 4];
                Color currentColor;
                for (var y = 0; y < (int) decoder.PixelHeight / 2; y++)
                for (var x = 0; x < (int) decoder.PixelWidth / 2; x++)
                {
                    currentColor =
                        PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                    PixelRetriever.ModifyPixel(quadrant1, x, y, currentColor, decoder.PixelWidth);
                }
                for (var y = 0; y < (int) decoder.PixelHeight / 2; y++)
                for (var x = (int) decoder.PixelWidth / 2; x < (int) decoder.PixelHeight; x++)
                {
                    currentColor =
                        PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                    PixelRetriever.ModifyPixel(quadrant2, x, y, currentColor, decoder.PixelWidth);
                }

                for (var y = (int) decoder.PixelHeight / 2; y < (int) decoder.PixelHeight; y++)
                for (var x = (int) decoder.PixelWidth / 2; x < (int) decoder.PixelHeight; x++)
                {
                    currentColor =
                        PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                    PixelRetriever.ModifyPixel(quadrant3, x, y, currentColor, decoder.PixelWidth);
                }


                for (var y = (int) decoder.PixelHeight / 2; y < (int) decoder.PixelHeight; y++)
                for (var x = 0; x < (int) decoder.PixelWidth / 2; x++)
                {
                    currentColor =
                        PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                    PixelRetriever.ModifyPixel(quadrant4, x, y, currentColor, decoder.PixelWidth);
                }


                var quadrants = new List<byte[]>
                {
                    quadrant1,
                    quadrant2,
                    quadrant3,
                    quadrant4
                };
                var encryptedImage = new byte[sourcePixels.Length];
                var ecryptionIndex = 0;
                foreach (var qaudrant in quadrants)
                foreach (var pixel in qaudrant)
                {
                    encryptedImage[ecryptionIndex] = pixel;
                    ecryptionIndex++;
                }
                return encryptedImage;
            }
        }
    }
}