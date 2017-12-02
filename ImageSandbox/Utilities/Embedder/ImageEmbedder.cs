using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Utilities.Converter;
using ImageSandbox.Utilities.Retriever;

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
        /// <param name="display">The display.</param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> EmbedImage(WriteableBitmap sourceImage, WriteableBitmap imageToEmbed,
            StorageFile sourceImageFile, StorageFile embedImageFile)
        {
            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var sourcePixelData = await PixelDataProvider(sourceImage, decoder);

                var sourcePixels = sourcePixelData.DetachPixelData();

                using (var embedfileStream = await embedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var embeddecoder = await BitmapDecoder.CreateAsync(embedfileStream);
                    var embedPixelData = await PixelDataProvider(imageToEmbed, embeddecoder);

                    var embedSourcePixels = embedPixelData.DetachPixelData();

                    WriteableBitmap bitmap = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

                    sourcePixels = EmbedBytes(sourcePixels, embedSourcePixels, embeddecoder.PixelWidth,
                        embeddecoder.PixelHeight, decoder.PixelWidth, decoder.PixelHeight);

                    if (sourcePixels == null) return bitmap;

                    using (var writeStream = bitmap.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    }

                    return bitmap;
                }
            }
        }


        /// <summary>
        /// Extracts the hidden image.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="sourceFile">The source file.</param>
        /// <returns></returns>
        public static async Task<Image> ExtractHiddenImage(WriteableBitmap sourceImage, StorageFile sourceFile)
        {
            var hiddenImage = new Image();
            using (var fileStream = await sourceFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var pixelData = await PixelDataProvider(sourceImage, decoder);

                var sourcePixels = pixelData.DetachPixelData();

                var extractedPixels =
                    await ExtractImageWithImage(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

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
        private static byte[] EmbedBytes(byte[] sourcePixels, byte[] embedPixels, uint embedImageWidth,
            uint embedImageHeight, uint sourceImageWidth, uint sourceImageHeight)
        {
            var sourceColor = Color.FromArgb(119, 119, 119, 119);

            if (embedImageWidth > sourceImageWidth || embedImageHeight > sourceImageHeight)
            {
                return null;
            }

            for (var y = 0; y < embedImageHeight; y++) { 
            for (var x = 0; x < embedImageWidth; x++) { 
                if (x == 0 && y == 0)
                {
                    PixelRetriever.ModifyPixel(sourcePixels, x, y, sourceColor, sourceImageWidth);
                }
                else if (x == 1 && y == 0)
                {
                    sourceColor = PixelRetriever.RetrieveColor(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                    sourceColor.R |= 0 << 0;
                    PixelRetriever.ModifyPixel(sourcePixels, x, y, sourceColor, sourceImageWidth);
                }
                else
                {
                    var embedColor = PixelRetriever.RetrieveColor(embedPixels, x, y, embedImageWidth, embedImageHeight);
                    if (embedColor.Equals(Colors.Black))
                    {
                        sourceColor =
                            PixelRetriever.RetrieveColor(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        sourceColor.B |= 0 << 0;
                        PixelRetriever.ModifyPixel(sourcePixels, x, y, sourceColor, sourceImageWidth);
                    }
                    else
                    {
                        sourceColor =
                            PixelRetriever.RetrieveColor(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                        sourceColor.B |= 1 << 0;
                        PixelRetriever.ModifyPixel(sourcePixels, x, y, sourceColor, sourceImageWidth);
                    }
                }
        }
    }
    return sourcePixels;
        }


        /// <summary>
        ///     Extracts the image with image.
        /// </summary>
        /// <param name="sourcePixels">The sourceImage pixels.</param>
        /// <param name="sourceImageWidth">Width of the sourceImage image.</param>
        /// <param name="sourceImageHeight">Height of the sourceImage image.</param>
        /// <returns></returns>
        private static async Task<byte[]> ExtractImageWithImage(byte[] sourcePixels, uint sourceImageWidth, uint sourceImageHeight)
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

                        sourceColor = PixelRetriever.RetrieveColor(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);
                        if (sourceColor.Equals(Color.FromArgb(119, 119, 119, 119)))
                        {
                            
                            return null;
                        }
                        break;
                    case 1 when j == 0:
                        var bitVal0 = sourceColor.R & 1;
                        if (bitVal0 == 1)
                        {
                            
                            return null;
                        }
                        break;
                    default:
                        sourceColor =
                            PixelRetriever.RetrieveColor(sourcePixels, i, j, sourceImageWidth, sourceImageHeight);

                        var bitVal = sourceColor.B & 1;
                        if (bitVal == 0)
                            PixelRetriever.ModifyPixel(imageExtract, i, j, Colors.Black, sourceImageWidth);
                        else
                            PixelRetriever.ModifyPixel(imageExtract, i, j, Colors.White, sourceImageWidth);

                        break;
                }
            }

            return imageExtract;
        }

     
    }
}