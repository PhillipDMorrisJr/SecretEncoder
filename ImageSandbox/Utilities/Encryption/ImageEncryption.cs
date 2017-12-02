using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using ImageSandbox.Utilities.Converter;
using ImageSandbox.Utilities.Retriever;

namespace ImageSandbox.Utilities.Encryption
{
    /// <summary>
    ///     Encrypts an image into an image.
    /// </summary>
    public static class ImageEncryption
    {
        public static async Task<Image> EncryptAsync(Image image, StorageFile sourceImageFile)
        {
            var imageToEncrypt = ToWriteableBitmapConverter.ConvertToWriteableBitmap(image);

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

                var encryptedImage = Encrypt(sourcePixels, decoder);

                using (var writeStream = imageToEncrypt.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(encryptedImage, 0, encryptedImage.Length);
                    image.Source = imageToEncrypt;
                    return image;
                }

                
            }
        }

        private static byte[] Encrypt(byte[] sourcePixels, BitmapDecoder decoder)
        {
            var quadrantSize = sourcePixels.Length / 4;
            var quadrant1 = new byte[quadrantSize];
            var quadrant2 = new byte[quadrantSize];
            var quadrant3 = new byte[quadrantSize];
            var quadrant4 = new byte[quadrantSize];


            var halfHeight = (int) decoder.PixelHeight / 2;
            var halfWidth = (int) decoder.PixelWidth / 2;
            var maxHeight = (int) decoder.PixelHeight;
            var maxWidth = (int) decoder.PixelWidth;

            quadrant1 = GroupPixels(sourcePixels, quadrant1, 0, halfWidth, 0, halfHeight, decoder.PixelWidth,
                decoder.PixelHeight);
            quadrant2 = GroupPixels(sourcePixels, quadrant2, halfWidth, maxWidth, 0, halfHeight, decoder.PixelWidth,
                decoder.PixelHeight);
            quadrant3 = GroupPixels(sourcePixels, quadrant3, 0, halfWidth, halfHeight, maxHeight, decoder.PixelWidth,
                decoder.PixelHeight);
            quadrant4 = GroupPixels(sourcePixels, quadrant4, halfWidth, maxWidth, halfHeight, maxHeight,
                decoder.PixelWidth, decoder.PixelHeight);


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


        private static byte[] GroupPixels(byte[] sourcePixels, byte[] quadrantPixels, int startX, int endX, int startY,
            int endY, uint width, uint height)
        {
            for (var y = startY; y < endY; y++)
            for (var x = startX; x < endX; x++)
            {
                var currentColor = PixelMap.RetrieveColor(sourcePixels, x, y, width, height);
                PixelMap.ModifyPixel(quadrantPixels, x, y, currentColor, width);
            }
            return sourcePixels;
        }
    }
}