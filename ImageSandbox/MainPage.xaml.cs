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
using ImageSandbox.Utilities;
using ImageSandbox.Utilities.Converter;
using ImageSandbox.Utilities.Embedder;


namespace ImageSandbox
{
    /// <summary>
    /// Application main page.
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
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
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
            this.imageDisplay = await ImageConverter.ConvertToImage(this.sourceImageFile);
             
        }

        private async void selectImageToEmbedWith_OnClick(object sender, RoutedEventArgs e)
        {
           this.embedImageFile = await this.selectSourceImageFile();
           this.embedDisplay = await ImageConverter.ConvertToImage(this.embedImageFile);
        }

        private async void embedImageButton_OnClick(object sender, RoutedEventArgs e)
        {
           this.encryptedImage = await ImageEmbedder.EmbedImage(this.sourceImage, this.embedImage);
        }

        private async void extractImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ImageEmbedder.ExtractHiddenImage(WriteableBitmapConverter.ConvertToWriteableBitmap(this.encryptedImage));
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