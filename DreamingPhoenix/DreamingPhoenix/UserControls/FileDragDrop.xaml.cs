using WizHat.DreamingPhoenix.AudioHandling;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.Extensions;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for FileDragDrop.xaml
    /// </summary>
    public partial class FileDragDrop : DialogControl, INotifyPropertyChanged
    {
        private List<string> droppedFiles = new List<string>();

        private Persistence.IPersistenceDataManager persistenceDataManager = new Persistence.PersistenceJsonDataManager();

        private string directoryPath;

        public string DirectoryPath
        {
            get { return directoryPath; }
            set
            {
                directoryPath = value;
                NotifyPropertyChanged();
            }
        }

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyPropertyChanged();
            }
        }

        private string audioName;

        public string AudioName
        {
            get { return audioName; }
            set
            {
                audioName = value;
                NotifyPropertyChanged();
            }
        }

        private string fileNameExt;

        public string FileNameExt
        {
            get { return fileNameExt; }
            set
            {
                fileNameExt = value;
                NotifyPropertyChanged();
            }
        }

        private Scene importedScene = null;

        public Scene ImportedScene
        {
            get { return importedScene; }
            set
            {
                importedScene = value;
                NotifyPropertyChanged();
            }
        }

        private string importedSceneStoragePath = null;

        public string ImportedSceneStoragePath
        {
            get { return importedSceneStoragePath; }
            set
            {
                importedSceneStoragePath = value;
                NotifyPropertyChanged();
            }
        }

        private bool isImportBusy = false;

        public bool IsImportBusy
        {
            get { return isImportBusy; }
            set { isImportBusy = value; NotifyPropertyChanged(); }
        }


        public FileDragDrop(List<string> filesDropped)
        {
            InitializeComponent();
            DataContext = this;

            droppedFiles = filesDropped;
            ProcessNextFile();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (droppedFiles.Count == 0)
                return;

            if (tglbtn_audioTrack.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new AudioTrack(droppedFiles[0], AudioName));
            }
            else if (tglbtn_soundEffect.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new SoundEffect(droppedFiles[0], AudioName));
            }

            droppedFiles.RemoveAt(0);
            ProcessNextFile();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            droppedFiles.RemoveAt(0);
            ProcessNextFile();
        }

        private async void ProcessNextFile()
        {
            if (droppedFiles.Count == 0)
            {
                Close();
                AppModel.Instance.SaveData();
                HelperFunctions.RefreshAudioListView();
                return;
            }

            string path = droppedFiles[0];
            if (!FileExtension.EndsWith(AppModel.Instance.ValidAudioExtensions, path) && !FileExtension.EndsWith(AppModel.Instance.ValidScenePackageExtensions, path))
            {
                droppedFiles.RemoveAt(0);
                ProcessNextFile();
                return;
            }

            if (FileExtension.EndsWith(AppModel.Instance.ValidScenePackageExtensions, path))
            {
                grd_sceneImport.Visibility = Visibility.Visible;
                grd_audio.Visibility = Visibility.Hidden;
                grd_convert.Visibility = Visibility.Hidden;

                
                ImportedScene = await persistenceDataManager.PeekScene(path);
            }
            else
            { 
                FileName = Path.GetFileNameWithoutExtension(path);
                AudioName = Path.GetFileNameWithoutExtension(path);
                FileNameExt = Path.GetFileName(path);
                DirectoryPath = Path.GetDirectoryName(path);

                if (AppModel.Instance.AudioManager.IsWrongSampleRate(path))
                {
                    grd_sceneImport.Visibility = Visibility.Hidden;
                    grd_audio.Visibility = Visibility.Hidden;
                    grd_convert.Visibility = Visibility.Visible;
                }
                else
                {
                    grd_audio.Visibility = Visibility.Visible;
                    grd_sceneImport.Visibility = Visibility.Hidden;
                    grd_convert.Visibility = Visibility.Hidden;
                }
            }
        }

        private void SoundEffect_Clicked(object sender, RoutedEventArgs e)
        {
            if (tglbtn_soundEffect == null || tglbtn_audioTrack == null)
                return;

            tglbtn_audioTrack.IsChecked = !tglbtn_soundEffect.IsChecked;
        }

        private void AudioTrack_Clicked(object sender, RoutedEventArgs e)
        {
            if (tglbtn_soundEffect == null || tglbtn_audioTrack == null)
                return;

            tglbtn_soundEffect.IsChecked = !tglbtn_audioTrack.IsChecked;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            droppedFiles.RemoveAt(0);
            ProcessNextFile();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            pgb_converting.Visibility = Visibility.Visible;
            string path = droppedFiles[0];
            btn_convert.IsEnabled = false;
            btn_cancleConvert.IsEnabled = false;

            Task.Run(() =>
            {
                if (!AppModel.Instance.AudioManager.ConvertToSampleRate(ref path))
                {
                    droppedFiles.RemoveAt(0);
                    pgb_converting.Dispatcher.Invoke(() =>
                    {
                        pgb_converting.Visibility = Visibility.Collapsed;

                        btn_convert.IsEnabled = true;
                        btn_cancleConvert.IsEnabled = true;
                    });
                    ProcessNextFile();
                    return;
                }

                droppedFiles[0] = path;
                FileName = Path.GetFileNameWithoutExtension(path);
                FileNameExt = Path.GetFileName(path);
                pgb_converting.Dispatcher.Invoke(() =>
                {
                    pgb_converting.Visibility = Visibility.Collapsed;
                    grd_audio.Visibility = Visibility.Visible;
                    grd_convert.Visibility = Visibility.Hidden;

                    btn_convert.IsEnabled = true;
                    btn_cancleConvert.IsEnabled = true;
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void btn_import_Click(object sender, RoutedEventArgs e)
        {
            IsImportBusy = true;
            await persistenceDataManager.ImportScene(droppedFiles[0], ImportedSceneStoragePath);
            IsImportBusy = false;
            droppedFiles.RemoveAt(0);
            ProcessNextFile();
            
        }

        private void btn_browseSceneStoragePath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ImportedSceneStoragePath = dialog.SelectedPath;
                }
            }
        }
    }
}
