using DreamingPhoenix.AudioHandling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for ImageSelection.xaml
    /// </summary>
    public partial class ImageSelection : UserControl, INotifyPropertyChanged
    {
        public event EventHandler OperationProcessed;

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



        public ImageSelection()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Scene.ImageSource = SelectedImage;
            string oldCacheID = Scene.ImageCacheID;
            Scene.ImageCacheID = Cache.CacheManager.Instance.GetNewCacheID();
            Cache.CacheManager.Instance.CleanUpCacheID(oldCacheID);
            Cache.CacheManager.Instance.SaveImageToCache(SelectedImage as BitmapSource, Scene.ImageCacheID);
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        public void SelectImageForScene(Scene scene)
        {
            Scene = scene;
            SelectedImage = Scene.ImageSource;
            tbox_webLink.Text = "";
        }

        private void ChooseImageFromDisk_Click(object sender, RoutedEventArgs e)
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
                var uri = new Uri("file://" + FileDialog.FileName);
                SelectedImage = new BitmapImage(uri);
            }
        }

        private void DownloadWebImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = new Uri(tbox_webLink.Text);
                SelectedImage = new BitmapImage(uri);
            }
            catch (Exception)
            {

            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        private void PasteLink_Click(object sender, RoutedEventArgs e)
        {
            tbox_webLink.Text = Clipboard.GetText(TextDataFormat.Text);
            DownloadWebImage_Click(this, null);
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
