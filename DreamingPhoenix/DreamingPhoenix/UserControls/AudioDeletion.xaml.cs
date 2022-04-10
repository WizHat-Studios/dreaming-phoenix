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
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for FileDragDrop.xaml
    /// </summary>
    public partial class AudioDeletion : UserControl, INotifyPropertyChanged
    {
        public event EventHandler OperationProcessed;

        private Audio audioToDelete;

        public Audio AudioToDelete
        {
            get { return audioToDelete; }
            set
            {
                audioToDelete = value;
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

        public AudioDeletion()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void AcceptDelete(Audio audioToDelete)
        {
            AudioToDelete = audioToDelete;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await Delete(audioToDelete);
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        public async void DeleteWithoutConfirmation(Audio audioToDelete)
        {
            await Delete(audioToDelete);
        }

        private async Task Delete(Audio audioToDelete)
        {
            AppModel.Instance.AudioList.Remove(audioToDelete);

            // Remove references in scenes
            foreach (Scene scene in AppModel.Instance.SceneList)
            {
                if (audioToDelete is AudioTrack)
                {
                    if (scene.SceneAudioTrack == (AudioTrack)audioToDelete)
                        scene.SceneAudioTrack = null;
                }
                else if (audioToDelete is SoundEffect)
                {
                    scene.SceneSoundEffects.Remove((SoundEffect)audioToDelete);
                }
            }

            AppModel.Instance.SaveData();
            await AppModel.Instance.ApplyFilterOptions(AppModel.Instance.Options.FilterOptions);
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }
    }
}
