using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Utilities;
using ImageSandbox.Utilities.Converter;
using ImageSandbox.Utilities.Embedder;
using ImageSandbox.Utilities.Encryption;
using ImageSandbox.Utilities.Retriever;


namespace ImageSandbox
{
    /// <summary>
    /// Application main page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        private WriteableBitmap embedImage;
        private WriteableBitmap imageResult;
        private WriteableBitmap sourceImage;
        private StorageFile sourceImageFile;
        private StorageFile embedImageFile;

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
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

                var pixelStream = this.imageResult.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                double x = await DotsPerInch.RetrieveX(this.sourceImageFile);
                double y = await DotsPerInch.RetrieveY(this.sourceImageFile);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) this.imageResult.PixelWidth,
                    (uint) this.imageResult.PixelHeight, x, y, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        //--------------------------------------------------------------//

        private async void selectSourceImage_OnClick(object sender, RoutedEventArgs e)
        {
            this.sourceImageFile = await this.selectSourceImageFile();
            this.imageDisplay = await ToImageConverter.Convert(this.sourceImageFile, this.imageDisplay);
            this.sourceImage = WriteableBitmapConverter.ConvertToWriteableBitmap(this.imageDisplay);

        }

        //---------------------------------------
        private async void selectImageToEmbedWith_OnClick(object sender, RoutedEventArgs e)
        {
           this.embedImageFile = await this.selectSourceImageFile();
           this.embedDisplay = await ToImageConverter.Convert(this.embedImageFile, this.embedDisplay);
           this.embedImage = WriteableBitmapConverter.ConvertToWriteableBitmap(this.embedDisplay);
        }
        //----------------------------------------------------------
    
        private async void embedImageButton_OnClick(object sender, RoutedEventArgs e)
        {
           this.encryptedImage = await ImageEmbedder.EmbedImage(this.sourceImage, this.embedImage, sourceImageFile, embedImageFile, new Image());
           this.imageResult = WriteableBitmapConverter.ConvertToWriteableBitmap(this.encryptedImage);
        }

        private async void extractImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.encryptedImage = await ImageEmbedder.ExtractHiddenImage(WriteableBitmapConverter.ConvertToWriteableBitmap(this.imageDisplay), this.sourceImageFile);
            this.imageResult = WriteableBitmapConverter.ConvertToWriteableBitmap(this.encryptedImage);

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

        private async void encryptImageButton_OnClick(object sender, RoutedEventArgs e)
        {
           var stuff = await ImageEncryption.EncryptAsync(this.imageDisplay, sourceImageFile);

           this.imageDisplay = await ToImageConverter.Convert(stuff, this.imageDisplay, sourceImage);
        }
    }
    #endregion
}