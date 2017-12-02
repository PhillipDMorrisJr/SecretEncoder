using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Utilities.Embedder
{
    /// <summary>
    ///     Encrypts an image into an image.
    /// </summary>
    public static class ImageEmbedder
    {
        /// <summary>
        ///     Embeds the image.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="imageToEmbed">The image to embed.</param>
        /// <param name="sourceImageFile">The source image file.</param>
        /// <param name="embedImageFile">The embed image file.</param>
        /// <returns></returns>
        public static async Task<Image> EmbedImage(WriteableBitmap sourceImage, WriteableBitmap imageToEmbed,
            StorageFile sourceImageFile, StorageFile embedImageFile)
        {
            Image embededImage = new Image();
            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var sourcePixelData = await PixelDataProvider(sourceImage, decoder);

                var sourcePixels = sourcePixelData.DetachPixelData();

                using (var embedfileStream = await embedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var embeddecoder = await BitmapDecoder.CreateAsync(embedfileStream);
                    var imageToEmbedPixelData = await PixelDataProvider(imageToEmbed, embeddecoder);

                    var embedSourcePixels = imageToEmbedPixelData.DetachPixelData();

                    var sourceCopy = EmbedImageWithImage(sourcePixels, embedSourcePixels,
                        embeddecoder.PixelWidth, embeddecoder.PixelHeight, decoder.PixelWidth, decoder.PixelHeight);
                    var image = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                    using (var writeStream = image.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(sourceCopy, 0, sourcePixels.Length);
                        embededImage.Source = image;
                    }
                }
            }
            return embededImage;
        }


        /// <summary>
        ///     Extracts the hidden image.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <returns></returns>
        public static async Task<Image> ExtractHiddenImage(WriteableBitmap sourceImage, StorageFile sourceFile)
        {
            var hiddenImage = new Image();
            using (var fileStream = await sourceFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(sourceImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(sourceImage.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                var extractedPixels = ExtractImageWithImage(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

                var embeddedImageBitmap = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

                using (var writeStream = embeddedImageBitmap.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(extractedPixels, 0, extractedPixels.Length);
                    hiddenImage.Source = embeddedImageBitmap;
                }
            }
            return hiddenImage;
        }

        private static async Task<PixelDataProvider> PixelDataProvider(WriteableBitmap aBitmapImage,
            BitmapDecoder decoder)
        {
            var transform = new BitmapTransform
            {
                ScaledWidth = Convert.ToUInt32(aBitmapImage.PixelWidth),
                ScaledHeight = Convert.ToUInt32(aBitmapImage.PixelHeight)
            };

            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );
            return pixelData;
        }

        /// <summary>
        ///     Embeds the image with image.
        /// </summary>
        /// <param name="sourcePixels">The sourceImage pixels.</param>
        /// <param name="embedPixels">The imageToEmbed pixels.</param>
        /// <param name="embedImageWidth">Width of the imageToEmbed image.</param>
        /// <param name="embedImageHeight">Height of the imageToEmbed image.</param>
        /// <param name="sourceImageWidth">Width of the sourceImage image.</param>
        /// <param name="sourceImageHeight">Height of the sourceImage image.</param>
        public static byte[] EmbedImageWithImage(byte[] sourcePixels, byte[] embedPixels, uint embedImageWidth,
            uint embedImageHeight, uint sourceImageWidth, uint sourceImageHeight)
        {
            var sourceColor = Color.FromArgb(119, 119, 119, 119);

            if (embedImageWidth > sourceImageWidth || embedImageHeight > sourceImageHeight) return null;


            for (var y = 0; y < embedImageHeight; y++)
            for (var x = 0; x < embedImageWidth; x++)
                if (x == 0 && y == 0)
                {
                    SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                }
                else if (x == 1 && y == 0)
                {
                    sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                    sourceColor.R |= 0 << 0;
                    SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                }
                else
                {
                    var embedColor = GetPixelBgra8(embedPixels, x, y, embedImageWidth, embedImageHeight);
                    if (embedColor.Equals(Colors.Black))
                    {
                        sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        sourceColor.B |= 0 << 0;
                        SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                    }
                    else
                    {
                        sourceColor = GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        sourceColor.B |= 1 << 0;
                        SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                    }
                }
            return sourcePixels;
        }

        private static Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            if (offset + 2 >= pixels.Length)
                return Colors.Black;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private static void SetPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            if (offset + 2 >= pixels.Length)
                return;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        /// <summary>
        ///     Extracts the image with image.
        /// </summary>
        /// <param name="sourcePixels">The sourceImage pixels.</param>
        /// <param name="sourceImageWidth">Width of the sourceImage image.</param>
        /// <param name="sourceImageHeight">Height of the sourceImage image.</param>
        /// <returns></returns>
        private static byte[] ExtractImageWithImage(byte[] sourcePixels, uint sourceImageWidth, uint sourceImageHeight)
        {
            var imageExtract = new byte[sourcePixels.Length];


            for (var i = 0; i < sourceImageHeight; i++)
            for (var j = 0; j < sourceImageWidth; j++)
            {
                //TODO: Create Constants
                Color sourceColor;
                switch (i)
                {
                    case 0 when j == 0:
                        sourceColor = GetPixelBgra8(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);
                        //TODO: CHEck if encrpyted post content dialog
                        break;
                    case 1 when j == 0:
                        var bitVal0 = sourceColor.R & 1;
                        if (bitVal0 == 1)
                        {
                            // throw new ArgumentException("This file should use text extraction");
                            //TODO: Call text Extraction
                        }
                        break;
                    default:
                        sourceColor = GetPixelBgra8(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);

                        var bitVal = sourceColor.B & 1;
                        if (bitVal == 0)
                            SetPixelBgra8(imageExtract, i, j, Colors.Black, sourceImageWidth, sourceImageHeight);
                        else
                            SetPixelBgra8(imageExtract, i, j, Colors.White, sourceImageWidth, sourceImageHeight);

                        break;
                }
            }

            return imageExtract;
        }

        private static async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }
    }
}