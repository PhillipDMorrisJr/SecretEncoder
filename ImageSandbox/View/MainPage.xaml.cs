using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Utilities.Converter;
using ImageSandbox.Utilities.Embedder;
using ImageSandbox.Utilities.Encryption;
using ImageSandbox.Utilities.Retriever;

namespace ImageSandbox
{
    /// <summary>
    ///     Application main page.
    /// </summary>
    public sealed partial class MainPage
    {
        #region Data members

        private WriteableBitmap _embedImage;
        private WriteableBitmap _imageResult;
        private WriteableBitmap _sourceImage;
        private StorageFile _sourceImageFile;
        private StorageFile _embedImageFile;

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private async Task<StorageFile> SelectSourceImageFile()
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

                var pixelStream = this._imageResult.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                var x = await DotsPerInch.RetrieveX(this._sourceImageFile);
                var y = await DotsPerInch.RetrieveY(this._sourceImageFile);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) this._imageResult.PixelWidth,
                    (uint) this._imageResult.PixelHeight, x, y, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        private async void selectSourceImage_OnClick(object sender, RoutedEventArgs e)
        {
            this._sourceImageFile = await this.SelectSourceImageFile();
            this.ImageDisplay = await ToImageConverter.Convert(this._sourceImageFile, this.ImageDisplay);
            this._sourceImage = WriteableBitmapConverter.ConvertToWriteableBitmap(this.ImageDisplay);
        }

        private async void selectImageToEmbedWith_OnClick(object sender, RoutedEventArgs e)
        {
            this._embedImageFile = await this.SelectSourceImageFile();
            this.EmbedDisplay = await ToImageConverter.Convert(this._embedImageFile, this.EmbedDisplay);
            this._embedImage = WriteableBitmapConverter.ConvertToWriteableBitmap(this.EmbedDisplay);
        }

        private async void embedImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.EncryptedImage =
                await ImageEmbedder.EmbedImage(this._sourceImage, this._embedImage, this._sourceImageFile,
                    this._embedImageFile, new Image());
            this._imageResult = WriteableBitmapConverter.ConvertToWriteableBitmap(this.EncryptedImage);
        }

        private async void extractImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.EncryptedImage = await ImageEmbedder.ExtractHiddenImage(
                    WriteableBitmapConverter.ConvertToWriteableBitmap(this.ImageDisplay), this._sourceImageFile);
                this._imageResult = WriteableBitmapConverter.ConvertToWriteableBitmap(this.EncryptedImage);
            }
            catch (Exception)
            {
                await customDialog("There are no secrets behind this image");
            }
        }

        private static async Task customDialog(string content)
        {
            var notImageEncryptedDialog = new ContentDialog {
                Content = content,
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Okay"
            };
            await notImageEncryptedDialog.ShowAsync();
        }

        private async void embedTextButton_OnClick(object sender, RoutedEventArgs e)
        {
            await customDialog("Functionality not implemeted");
        }

        private async void extractTextButton_OnClick(object sender, RoutedEventArgs e)
        {
            await customDialog("Functionality not implemeted");
        }

        private async void encryptImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var encryptedPixels = await ImageEncryption.EncryptAsync(this.ImageDisplay, this._sourceImageFile);

                this.ImageDisplay =
                    await ToImageConverter.Convert(encryptedPixels, this.ImageDisplay, this._sourceImage);
            }
            catch (Exception)
            {
                await customDialog("Image cannot be encrypted");
            }
        }

        #endregion
    }
}