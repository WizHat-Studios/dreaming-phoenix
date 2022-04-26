using WizHat.DreamingPhoenix.Styles.ComboBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.AudioProperties;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for AudioTrackProperties.xaml
    /// </summary>
    public partial class AudioTrackProperties : UserControl, INotifyPropertyChanged
    {
        public AudioTrack Track { get; set; }
        private List<Audio> tracks = new List<Audio>();

        public List<Audio> Tracks
        {
            get { return tracks; }
            set
            {
                tracks = value;
                NotifyPropertyChanged();
            }
        }

        public AudioTrackProperties(AudioTrack audioTrack)
        {
            InitializeComponent();
            this.DataContext = this;
            Track = audioTrack;
            Tracks = AppModel.Instance.AudioList.Where(a => a.GetType() == typeof(AudioTrack) && a != Track).ToList();
        }       

        private void tbx_audioFile_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files[0] != "" && FileExtension.EndsWith(AppModel.Instance.ValidAudioExtensions, files[0]))
                e.Handled = true;
        }

        private void tbx_audioFile_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files[0] != "" && FileExtension.EndsWith(AppModel.Instance.ValidAudioExtensions, files[0]))
                Track.AudioFile = files[0];
        }

        private void btn_selectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                RestoreDirectory = true,
                DefaultExt = "mp3",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Track.AudioFile)),
                Filter = "Audio Files|" + FileExtension.GetDialogExtensions(AppModel.Instance.ValidAudioExtensions)
            };
            FileDialog.Title = "Select Audio File";
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Track.AudioFile = FileDialog.FileName;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Track == null)
                return;

            AppModel.Instance.AudioManager.PlayAudio(Track);
        }

        private void RemoveNextTrack_Click(object sender, RoutedEventArgs e)
        {
            Track.NextAudioTrack = null;
        }
        
        private void RemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            Track.Category = Category.Default;
        }
        
        private async void DeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            await mainWindow.ShowDialog(new AudioDeletion(Track));
            mainWindow.grid_selectedAudioProperties.Children.Clear();
        }

        private async void DeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            await mainWindow.ShowDialog(new AudioDeletion(Track));
            mainWindow.grid_selectedAudioProperties.Children.Clear();
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            Tag tag = ((Button)sender).Tag as Tag;
            Track.Tags.Remove(tag);
        }

        private void AddNewTag_Click(object sender, RoutedEventArgs e)
        {
            if (Track.Tags == null)
                Track.Tags = new ObservableCollection<Tag>();
            Track.Tags.Add(new Tag() { Text = "New Tag" });
        }

        private async void SelectNextAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            Audio newNextAudioTrack = await mainWindow.ShowDialog<Audio>(new AudioSelection(typeof(AudioTrack), Track.NextAudioTrack));

            if (newNextAudioTrack != null)
                Track.NextAudioTrack = (AudioTrack)newNextAudioTrack;
        }
        
        private async void SelectCategory_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            Category newCategory = await mainWindow.ShowDialog<Category>(new AudioCategorySelection(Track.Category));

            if (newCategory != null)
                Track.Category = newCategory;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
