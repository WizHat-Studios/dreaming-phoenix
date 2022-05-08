using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Cache;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for ImageSelection.xaml
    /// </summary>
    public partial class ImageSelection : DialogControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ImageSource selectedImage;

        public ImageSource SelectedImage
        {
            get { return selectedImage; }
            set { selectedImage = value; NotifyPropertyChanged(); }
        }

        private Scene scene;

        public Scene Scene
        {
            get { return scene; }
            set { scene = value; NotifyPropertyChanged(); }
        }

        private bool isDownloadBusy = false;

        public bool IsDownloadBusy
        {
            get { return isDownloadBusy; }
            set { isDownloadBusy = value; NotifyPropertyChanged(); }
        }




        public ImageSelection(Scene scene)
        {
            InitializeComponent();
            this.DataContext = this;

            Scene = scene;
            SelectedImage = Scene.ImageSource;
            tbox_webLink.Text = "";
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Scene.ImageSource = SelectedImage;
            string oldCacheID = Scene.ImageCacheID;
            Scene.ImageCacheID = CacheManager.Instance.GetNewCacheID();
            CacheManager.Instance.CleanUpCacheID(oldCacheID);
            CacheManager.Instance.SaveImageToCache(SelectedImage as BitmapSource, Scene.ImageCacheID);
            Close();
        }

        private async void ChooseImageFromDisk_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                RestoreDirectory = true,
                DefaultExt = "png",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "Image Files | *.png; *.jpg; *.jpeg; *.bmp; *.gif"
            };
            FileDialog.Title = "Select Image File";
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var uri = new Uri("file://" + FileDialog.FileName);
                    SelectedImage = new BitmapImage(uri);
                }
                catch (Exception)
                {
                    await MainWindow.Current.ShowDialog(new ErrorMessage("Looks like this image is corrupted or its type is not supported. Please use a different image.", "IMAGE IS UNKNOWN TYPE"));
                }
                
            }
        }

        private async void DownloadWebImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsDownloadBusy = true;
                var uri = new Uri(tbox_webLink.Text);

                BitmapImage bitmap = null;
                var httpClient = new HttpClient();

                using (var response = await httpClient.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = new MemoryStream())
                        {
                            await response.Content.CopyToAsync(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                            bitmap.Freeze();
                        }
                    }
                }

                SelectedImage = bitmap;
            }
            catch (Exception)
            {
                await MainWindow.Current.ShowDialog(new ErrorMessage("Looks like this image is corrupted or its type is not supported. Please use a different image.", "IMAGE IS UNKNOWN TYPE"));
            }
            IsDownloadBusy = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasteLink_Click(object sender, RoutedEventArgs e)
        {
            tbox_webLink.Text = Clipboard.GetText(TextDataFormat.Text);
        }

        private void tbox_webLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tblock_webLinkHint == null)
                return;

            if (string.IsNullOrEmpty(tbox_webLink.Text))
                tblock_webLinkHint.Visibility = Visibility.Visible;
            else
                tblock_webLinkHint.Visibility = Visibility.Collapsed;
        }
    }
}
