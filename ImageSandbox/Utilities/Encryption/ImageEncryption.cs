using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Encrypts an image into an image.
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
                var writeableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                byte[] quadrant1 = new byte[sourcePixels.Length/4];
                byte[] quadrant2 = new byte[sourcePixels.Length / 4];
                byte[] quadrant3 = new byte[sourcePixels.Length / 4];
                byte[] quadrant4 = new byte[sourcePixels.Length / 4];
                Color currentColor;
                for (int y = 0; y < ((int) decoder.PixelHeight / 2); y++)
                {
                    for (int x = 0; x < ((int)decoder.PixelWidth / 2); x++)
                    {
                        currentColor = PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                        PixelRetriever.PixelModifier(quadrant1, x, y, currentColor, decoder.PixelWidth);

                    }
                }
                for (int y = 0; y < ((int)decoder.PixelHeight / 2); y++)
                {
                    for (int x = ((int)decoder.PixelWidth / 2); x < (int)decoder.PixelHeight; x++)
                    {
                        currentColor = PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                        PixelRetriever.PixelModifier(quadrant2, x, y, currentColor, decoder.PixelWidth);
                    }
                }

                for (int y = ((int)decoder.PixelHeight / 2); y < (int)decoder.PixelHeight; y++)
                {
                    for (int x = ((int)decoder.PixelWidth / 2); x < (int)decoder.PixelHeight; x++)
                    {
                        currentColor = PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                        PixelRetriever.PixelModifier(quadrant3, x, y, currentColor, decoder.PixelWidth);
                    }
                }

                
                for (int y = ((int)decoder.PixelHeight / 2); y < (int)decoder.PixelHeight; y++)
                {
                    for (int x = 0; x < ((int)decoder.PixelWidth / 2); x++)
                    {
                        currentColor = PixelRetriever.RetrieveColor(sourcePixels, x, y, decoder.PixelWidth, decoder.PixelHeight);
                        PixelRetriever.PixelModifier(quadrant4, x, y, currentColor, decoder.PixelWidth);
                    }
                }



                List<byte[]> quadrants = new List<byte[]>
                {
                    quadrant1,quadrant2,quadrant3,quadrant4,
                };
                byte[] encryptedImage = new byte[sourcePixels.Length];
                int ecryptionIndex = 0;
                foreach (var qaudrant in quadrants)
                {
                    foreach (byte pixel in qaudrant)
                    {
                        encryptedImage[ecryptionIndex] = pixel;
                        ecryptionIndex++;
                    }
                }
                return encryptedImage;

            }
        }
    }
}
