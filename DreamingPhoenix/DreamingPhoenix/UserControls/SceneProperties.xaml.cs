using DreamingPhoenix.AudioHandling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for SceneProperties.xaml
    /// </summary>
    public partial class SceneProperties : UserControl, INotifyPropertyChanged
    {
        public Scene Scene { get; set; }

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

        private List<Audio> soundEffects = new List<Audio>();

        public List<Audio> SoundEffects
        {
            get { return soundEffects; }
            set
            {
                soundEffects = value;
                NotifyPropertyChanged();
            }
        }

        private bool areAudioFilesValid = true;

        public bool AreAudioFilesValid
        {
            get { return areAudioFilesValid; }
            set { areAudioFilesValid = value; NotifyPropertyChanged(); }
        }


        public SceneProperties(Scene scene)
        {
            InitializeComponent();
            this.DataContext = this;
            Scene = scene;
            Tracks = AppModel.Instance.AudioList.Where(a => a.GetType() == typeof(AudioTrack)).ToList();
            SoundEffects = AppModel.Instance.AudioList.Where(a => a.GetType() == typeof(SoundEffect)).ToList();

            foreach (SoundEffect sf in Scene.SceneSoundEffects)
            {
                if (!sf.IsAudioFilePathValid)
                    AreAudioFilesValid = false;
            }

            if (Scene.SceneAudioTrack != null && !Scene.SceneAudioTrack.IsAudioFilePathValid)
                AreAudioFilesValid = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void AddSoundEffect_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_soundEffects.SelectedItem != null)
                Scene.SceneSoundEffects.Add(cmb_soundEffects.SelectedItem as SoundEffect);
        }

        private void cmb_audioTrack_Loaded(object sender, RoutedEventArgs e)
        {
            cmb_audioTrack.SelectedItem = Scene.SceneAudioTrack;
        }

        private void cmb_audioTrack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmb_audioTrack.SelectedIndex == -1)
                return;

            Scene.SceneAudioTrack = (AudioTrack)cmb_audioTrack.SelectedItem;
        }

        private void RemoveSoundEffect_Click(object sender, RoutedEventArgs e)
        {
            Scene.SceneSoundEffects.Remove((sender as Button).Tag as SoundEffect);
        }

        private void RemoveAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            Scene.SceneAudioTrack = null;
            cmb_audioTrack.SelectedItem = null;
        }

        private void DeleteScene_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.SceneDeletionPanelVisibility = Visibility.Visible;
            mainWindow.uc_sceneDeletion.AcceptDelete((Scene)Scene);
            mainWindow.grid_selectedAudioProperties.Children.Clear();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Scene == null)
                return;

            AppModel.Instance.AudioManager.PlayScene(Scene);
        }

        private async void ReloadImage_Click(object sender, RoutedEventArgs e)
        {
            //await Scene.LoadImage(Scene.ImageUri);
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ImageSelectionPanelVisibility = Visibility.Visible;
            mainWindow.uc_imageSelection.SelectImageForScene(Scene);
            //mainWindow.uc_imageSelection.AcceptDelete((Scene)Scene);

            //Guid.NewGuid().ToString("n");
        }

        private void BackgroundImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Scene.ImageSource != null)
            {
                btn_removeBackground.Visibility = Visibility.Visible;
            }
        }

        private void BackgroundImage_MouseLeave(object sender, MouseEventArgs e)
        {
            btn_removeBackground.Visibility = Visibility.Collapsed;
        }

        private void btn_removeBackground_Click(object sender, RoutedEventArgs e)
        {
            Scene.ImageSource = null;
            Cache.CacheManager.Instance.CleanUpCacheID(Scene.ImageCacheID);
            Scene.ImageCacheID = null;
        }
    }
}
