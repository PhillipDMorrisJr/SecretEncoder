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
using ImageSandbox.Utilities;
using ImageSandbox.Utilities.Converter;
using ImageSandbox.Utilities.Embedder;
using ImageSandbox.Utilities.Encryption;
using ImageSandbox.Utilities.Retriever;

namespace ImageSandbox
{
    /// <summary>
    ///     Application main page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Constructors

        public MainPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Data members

        private WriteableBitmap _embedImage;
        private WriteableBitmap _imageResult;
        private WriteableBitmap _sourceImage;
        private StorageFile _sourceImageFile;
        private StorageFile _embedImageFile;

        #endregion

        #region Methods

        private async Task<StorageFile> SelectSourceImageFile()
        {
            var openPicker = new FileOpenPicker
            {
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
            SaveWritableBitmap();
        }

        private async void SaveWritableBitmap()
        {
            var fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
            var savefile = await fileSavePicker.PickSaveFileAsync();

            if (savefile != null)
            {
                var stream = await savefile.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                var pixelStream = _imageResult.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                var x = await DotsPerInch.RetrieveX(_sourceImageFile);
                var y = await DotsPerInch.RetrieveY(_sourceImageFile);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) _imageResult.PixelWidth,
                    (uint) _imageResult.PixelHeight, x, y, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        //--------------------------------------------------------------//

        private async void selectSourceImage_OnClick(object sender, RoutedEventArgs e)
        {
            _sourceImageFile = await SelectSourceImageFile();
            ImageDisplay = await ToImageConverter.Convert(_sourceImageFile, ImageDisplay);
            _sourceImage = WriteableBitmapConverter.ConvertToWriteableBitmap(ImageDisplay);
        }

        //---------------------------------------
        private async void selectImageToEmbedWith_OnClick(object sender, RoutedEventArgs e)
        {
            _embedImageFile = await SelectSourceImageFile();
            EmbedDisplay = await ToImageConverter.Convert(_embedImageFile, EmbedDisplay);
            _embedImage = WriteableBitmapConverter.ConvertToWriteableBitmap(EmbedDisplay);
        }
        //----------------------------------------------------------

        private async void embedImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            EncryptedImage =
                await ImageEmbedder.EmbedImage(_sourceImage, _embedImage, _sourceImageFile, _embedImageFile, new Image());
            _imageResult = WriteableBitmapConverter.ConvertToWriteableBitmap(EncryptedImage);
        }

        private async void extractImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            EncryptedImage =
                await ImageEmbedder.ExtractHiddenImage(WriteableBitmapConverter.ConvertToWriteableBitmap(ImageDisplay),
                    _sourceImageFile);
            _imageResult = WriteableBitmapConverter.ConvertToWriteableBitmap(EncryptedImage);
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
            var stuff = await ImageEncryption.EncryptAsync(ImageDisplay, _sourceImageFile);

            ImageDisplay = await ToImageConverter.Convert(stuff, ImageDisplay, _sourceImage);
        }
    }

    #endregion
}