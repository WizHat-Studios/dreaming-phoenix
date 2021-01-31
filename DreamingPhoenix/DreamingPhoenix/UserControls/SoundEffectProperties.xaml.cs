using DreamingPhoenix.AudioHandling;
using DreamingPhoenix.Styles.ComboBox;
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

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for SoundEffectProperties.xaml
    /// </summary>
    public partial class SoundEffectProperties : UserControl, INotifyPropertyChanged
    {
        public SoundEffect Sound { get; set; }

        public SoundEffectProperties(SoundEffect soundEffect)
        {
            InitializeComponent();
            this.DataContext = this;
            Sound = soundEffect;
        }

        private void tbx_soundFile_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files[0] != "" && FileExtension.EndsWith(AppModel.Instance.ValidAudioExtensions, files[0]))
                e.Handled = true;
        }

        private void tbx_soundFile_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files[0] != "" && FileExtension.EndsWith(AppModel.Instance.ValidAudioExtensions, files[0]))
                Sound.AudioFile = files[0];
        }

        private void btn_selectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                RestoreDirectory = true,
                DefaultExt = "mp3",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = System.IO.Path.GetDirectoryName(Sound.AudioFile),
                Filter = "Audio Files|" + FileExtension.GetDialogExtensions(AppModel.Instance.ValidAudioExtensions)
            };
            FileDialog.Title = "Select Audio File";
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Sound.AudioFile = FileDialog.FileName;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Sound == null)
                return;

            AppModel.Instance.AudioManager.PlayAudio(Sound);
        }

        private void DeleteSound_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.AudioList.Remove(Sound);
            this.Visibility = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
