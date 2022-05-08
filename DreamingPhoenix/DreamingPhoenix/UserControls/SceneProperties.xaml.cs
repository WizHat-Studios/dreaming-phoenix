using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.Persistence;

namespace WizHat.DreamingPhoenix.UserControls
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

        private bool isExportBusy = false;

        public bool IsExportBusy
        {
            get { return isExportBusy; }
            set { isExportBusy = value; NotifyPropertyChanged(); }
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

        private async void AddSoundEffect_Click(object sender, RoutedEventArgs e)
        {
            Scene.SceneSoundEffects = new((await ItemSelectionList.SelectAudios(Scene.SceneSoundEffects.Cast<Audio>().ToList(), typeof(SoundEffect))).Cast<SoundEffect>().ToList());
        }

        private void RemoveSoundEffect_Click(object sender, RoutedEventArgs e)
        {
            Scene.SceneSoundEffects.Remove((sender as Button).Tag as SoundEffect);
        }

        private void RemoveAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            Scene.SceneAudioTrack = null;
        }

        private async void DeleteScene_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Current.ShowDialog(new SceneDeletion(Scene));
            MainWindow.Current.grid_selectedAudioProperties.Children.Clear();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Scene == null)
                return;

            AppModel.Instance.AudioManager.PlayScene(Scene);
        }

        private async void ReloadImage_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Current.ShowDialog(new ImageSelection(Scene));
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
            CacheManager.Instance.CleanUpCacheID(Scene.ImageCacheID);
            Scene.ImageCacheID = null;
        }

        private async void SelectAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            Scene.SceneAudioTrack = (AudioTrack)await ItemSelectionList.SelectAudio(Scene.SceneAudioTrack, typeof(AudioTrack));
        }

        private async void ExportScene_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "ZIP File (*.zip)|*.zip|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = ".zip";
            saveFileDialog.FileName = Scene.SceneName;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsExportBusy = true;
                IPersistenceDataManager persistenceDataManager = new PersistenceJsonDataManager();
                bool success = await persistenceDataManager.ExportScene(saveFileDialog.FileName, Scene);
                IsExportBusy = false;

                if (success)
                {
                    grid_exportedSuccess.Visibility = Visibility.Visible;
                    await Task.Delay(5000);
                    grid_exportedSuccess.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_exportedFailed.Visibility = Visibility.Visible;
                    await MainWindow.Current.ShowDialog(new ErrorMessage("The export of the scene failed. Please make sure the directory you're trying to export to is not protected and the scene is properly configured."));
                    await Task.Delay(5000);
                    grid_exportedFailed.Visibility = Visibility.Collapsed;
                }

               

            }
        }
    }
}
