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
using ImageSandbox.Model.Encryption;

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
        private WriteableBitmap ImageResult;
        private WriteableBitmap sourceImage;
        private StorageFile sourceImageFile;
        private StorageFile embedImageFile;

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

        private void saveButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SaveWritableBitmap();
        }

        private async void SaveWritableBitmap()
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

                var pixelStream = this.ImageResult.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) this.ImageResult.PixelWidth,
                    (uint) this.ImageResult.PixelHeight, this.dpiX, this.dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }
        

        private async void selectSourceImage_OnClick(object sender, RoutedEventArgs e)
        {
            this.sourceImageFile = await this.selectSourceImageFile();
            await UpdateImage(this.imageDisplay, this.sourceImageFile);
            this.sourceImage = this.modifiedImage;
        }
        private async void selectImageToEmbedWith_OnClick(object sender, RoutedEventArgs e)
        {
            this.embedImageFile = await this.selectSourceImageFile();

            await UpdateImage(this.embedDisplay, this.embedImageFile);
            this.embedImage = this.modifiedImage;
        }

        private async void embedImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            
            var copyBitmapImage = this.sourceImage;

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                var embedcopyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(embedImageFile);

                using (var embedfileStream = await embedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var embeddecoder = await BitmapDecoder.CreateAsync(embedfileStream);
                    var embedtransform = new BitmapTransform
                    {
                        ScaledWidth = Convert.ToUInt32(embedcopyBitmapImage.PixelWidth),
                        ScaledHeight = Convert.ToUInt32(embedcopyBitmapImage.PixelHeight)
                    };

                    var pixelData2 = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        embedtransform,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage
                    );

                    var embedSourcePixels = pixelData2.DetachPixelData();

                    ImageEmbedder.EmbedImageWithImage(sourcePixels, embedSourcePixels, embeddecoder.PixelWidth, embeddecoder.PixelHeight, decoder.PixelWidth, decoder.PixelHeight);

                    this.modifiedImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                        this.encryptedImage.Source = this.modifiedImage;
                        this.ImageResult = this.modifiedImage;
                    }
                }

            }
        }
       
        private async void extractImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var copyBitmapImage = this.sourceImage;

            using (var fileStream = await sourceImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                byte[] extractedPixels = ImageEmbedder.ExtractImageWithImage(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

                    this.modifiedImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                    {
                        await writeStream.WriteAsync(extractedPixels, 0, extractedPixels.Length);
                        this.encryptedImage.Source = this.modifiedImage;
                        this.ImageResult = this.modifiedImage;
                    }
                }
            }


  private async Task UpdateImage(Image imageToUpdate, StorageFile imageFile)
        {
            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(imageFile);
            using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = new BitmapTransform
                {
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

                this.modifiedImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await UpdateImageSource(sourcePixels, imageToUpdate);
            }
        }

        private async Task UpdateImageSource(byte[] sourcePixels, Image originalImage)
        {
            using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                originalImage.Source = this.modifiedImage;
            }
        }




        
        
      
















        //--------------------------------------------------------------------------------------------------------------//
        private void embedTextButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void extractTextButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        //--------------------------------------------------------------------------------------------------------------//

    }
    #endregion
}