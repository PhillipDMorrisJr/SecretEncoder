using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageSandbox
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        private double dpiX;
        private double dpiY;
        private WriteableBitmap modifiedImage;
        private WriteableBitmap embedImage;

        private bool isEmbedSet;

        #endregion

        #region Constructors

        public MainPage()
        {
            this.isEmbedSet = false;
            this.InitializeComponent();

            this.modifiedImage = null;
            this.dpiX = 0;
            this.dpiY = 0;
        }

        #endregion

        #region Methods

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var sourceImageFile = await this.selectSourceImageFile();
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                this.dpiX = decoder.DpiX;
                this.dpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                this.giveImageRedTint(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

                this.modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.imageDisplay.Source = this.modifiedImage;
                }
            }
        }

        private void giveImageRedTint(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            for (var i = 0; i < imageHeight; i++)
            {
                for (var j = 0; j < imageWidth; j++)
                {
                    var pixelColor = this.GetPixelBgra8(sourcePixels, i, j, imageWidth, imageHeight);

                    pixelColor.R = 255;

                    this.SetPixelBgra8(sourcePixels, i, j, pixelColor, imageWidth, imageHeight);
                }
            }
        }

        private void embedImageWithImage(byte[] sourcePixels, byte[] embedPixels, uint embedImageWidth, uint embedImageHeight, uint sourceImageWidth, uint sourceImageHeight)
        {
            int y;
            int x = y = 0;
            Color sourceColor = Color.FromArgb(160, 160, 160, 160);

            //0 is r 1 is g 2 is b
            int sourceByte = 0;
            for (var i = 0; i < embedImageHeight; i++)
            {
                for (var j = 0; j < embedImageWidth; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        
                        this.SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                        sourceColor = this.GetPixelBgra8(sourcePixels, x, y, sourceImageWidth, sourceImageHeight);
                    }
                    else
                    {
                        var embedColor = this.GetPixelBgra8(embedPixels, i, j, embedImageWidth, embedImageHeight);

                        if (embedColor == Colors.Black)
                        {
                            switch (sourceByte)
                            {
                                case 0:
                                    sourceColor.R |= (0 << 0);
                                    sourceByte = 1;
                                    break;
                                case 1:
                                    sourceColor.G |= (0 << 0);
                                    sourceByte = 2;
                                    break;
                                case 2:
                                    sourceColor.B |= (0 << 0);
                                    if (x < embedImageWidth)
                                    {
                                        x++;
                                    }
                                    else
                                    {
                                        x = 0;
                                    }
                                    if (y < embedImageHeight)
                                    {
                                        y++;
                                    }
                                    if (x == embedImageWidth && y == embedImageHeight)
                                    {
                                        break;
                                    }
                                    sourceByte = 0;
                                    break;
                            }
                        }
                        else if (embedColor == Colors.White)
                        {
                            switch (sourceByte)
                            {
                                case 0:
                                    sourceColor.R |= (1 << 0);
                                    sourceByte = 1;
                                    break;
                                case 1:
                                    sourceColor.G |= (1 << 0);
                                    sourceByte = 2;
                                    break;
                                case 2:
                                    sourceColor.B |= (1 << 0);
                                    this.SetPixelBgra8(sourcePixels, x, y, sourceColor, sourceImageWidth, sourceImageHeight);
                                    sourceColor = this.GetPixelBgra8(sourcePixels, x, y, embedImageWidth, embedImageHeight);
                                    if (x < embedImageWidth)
                                    {
                                        x++;
                                    }
                                    else
                                    {
                                        x = 0;
                                    }
                                    if (y < embedImageHeight)
                                    {
                                        y++;
                                    }
                                    if (x == embedImageWidth && y == embedImageHeight)
                                    {
                                        break;
                                    }
                                    sourceByte = 0;
                                    
                                    break;
                            }

                        }
                    }

                    

                    
                }
            }
        }



        private async Task<StorageFile> selectSourceImageFile()
        {
            var openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }

        public Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        public void SetPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            this.saveWritableBitmap();
        }

        private async void saveWritableBitmap()
        {
            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
            var savefile = await fileSavePicker.PickSaveFileAsync();

            if (savefile != null)
            {
                var stream = await savefile.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                var pixelStream = this.modifiedImage.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) this.modifiedImage.PixelWidth,
                    (uint) this.modifiedImage.PixelHeight, this.dpiX, this.dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        private async  void embedFileButton_Click(object sender, RoutedEventArgs e)
        {
            var sourceImageFile = await this.selectSourceImageFile();
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                this.dpiX = decoder.DpiX;
                this.dpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                

                //////////
                /// 
                var embedImageFile = await this.selectSourceImageFile();
                var embedcopyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);
                double dpx;
                double dpy;

                using (var embedfileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var embeddecoder = await BitmapDecoder.CreateAsync(embedfileStream);
                    var embedtransform = new BitmapTransform {
                        ScaledWidth = Convert.ToUInt32(embedcopyBitmapImage.PixelWidth),
                        ScaledHeight = Convert.ToUInt32(embedcopyBitmapImage.PixelHeight)
                    };

                    dpx = decoder.DpiX;
                    dpy = decoder.DpiY;

                    var pixelData2 = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        embedtransform,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage
                    );

                    var embedSourcePixels = pixelData2.DetachPixelData();

                    this.embedImageWithImage(sourcePixels, embedSourcePixels,embeddecoder.PixelWidth, embeddecoder.PixelHeight, decoder.PixelWidth, decoder.PixelHeight);

                    this.modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                    using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                        this.imageDisplay.Source = this.modifiedImage;
                    }
                }

            }
        }

        private async void selectEmbedFileButton_Click(object sender, RoutedEventArgs e)
        {
            var sourceImageFile = await this.selectSourceImageFile();
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(sourceImageFile);

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                this.dpiX = decoder.DpiX;
                this.dpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );
                var sourcePixels = pixelData.DetachPixelData();

                this.embedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = this.embedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.embedDisplay.Source = this.embedImage;
                }
                this.isEmbedSet = true;
            }
        }

        #endregion
    }
}