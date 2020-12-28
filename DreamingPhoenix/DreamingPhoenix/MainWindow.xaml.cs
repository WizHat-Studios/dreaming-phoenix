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
using DreamingPhoenix.AudioHandling;

namespace DreamingPhoenix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        private Visibility settingsPanelVisibility = Visibility.Hidden;

        public Visibility SettingsPanelVisibility
        {
            get { return settingsPanelVisibility; }
            set { settingsPanelVisibility = value; NotifyPropertyChanged(); }
        }

        private Visibility audioDropPanelVisibility = Visibility.Hidden;

        public Visibility AudioDropPanelVisibility
        {
            get { return audioDropPanelVisibility; }
            set { audioDropPanelVisibility = value; NotifyPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();

            grid_AppOptions.Visibility = Visibility.Visible;
            grid_DropPanel.Visibility = Visibility.Visible;

            uc_DropPanel.AudioFilesProcessed += (s, e) => AudioDropPanelVisibility = Visibility.Hidden;

            this.DataContext = this;
            AppModel.Instance.AudioManager.CurrentlyPlayingAudioTrack.AudioTrackTick += (currSecond, totalSeconds) =>
            {
                pgb_audioTrack.Dispatcher.Invoke(() =>
                {
                    pgb_audioTrack.Value = currSecond;
                    pgb_audioTrack.Maximum = totalSeconds;
                });
            };
        }

        private void PlayAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            AudioTrack track = (AudioTrack)((Button)sender).DataContext;
            AppModel.Instance.AudioManager.PlayAudio(track);
            btn_PauseAudioTrack.Visibility = Visibility.Visible;
            btn_PlayAudioTrack.Visibility = Visibility.Collapsed;
        }

        private async void PlayPauseAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            if (!await AppModel.Instance.AudioManager.PausePlayAudioTrack())
                return;
            btn_PauseAudioTrack.Visibility = ChangeVisibility(btn_PauseAudioTrack.Visibility, true);
            btn_PlayAudioTrack.Visibility = ChangeVisibility(btn_PlayAudioTrack.Visibility, true);
        }

        private Visibility ChangeVisibility(Visibility visibility, bool collapsed)
        {
            if (visibility == Visibility.Visible)
            {
                if (collapsed)
                    return Visibility.Collapsed;
                else
                    return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        private void PlaySoundEffect_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            SoundEffect sound = (SoundEffect)((Button)sender).DataContext;
            AppModel.Instance.AudioManager.PlayAudio(sound);
        }

        private void StopSoundEffect_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            AppModel.Instance.AudioManager.StopAudio((PlayableAudio)((Button)sender).DataContext);
        }

        private void RemoveAudio_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            AppModel.Instance.AudioList.Remove((Audio)((Button)sender).DataContext);
            grid_selectedAudioProperties.Children.Clear();
        }

        private void AudioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;

            switch ((Audio)((ListBox)sender).SelectedItem)
            {
                case AudioTrack at:
                    grid_selectedAudioProperties.Children.Clear();
                    grid_selectedAudioProperties.Children.Add(new UserControls.AudioTrackProperties(at));
                    break;
                case SoundEffect se:
                    grid_selectedAudioProperties.Children.Clear();
                    grid_selectedAudioProperties.Children.Add(new UserControls.SoundEffectProperties(se));
                    break;
                default:
                    throw new NotSupportedException("The given type is not supported for adjustable settings");
            }
        }

        protected void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {          
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                if (!FileExtension.EndsWith(AppModelInstance.ValidAudioExtensions, file))
                {
                    return;
                }
            }

            AudioDropPanelVisibility = Visibility.Visible;
            this.Activate();
            uc_DropPanel.OnDrop(files.ToList());
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                if (!FileExtension.EndsWith(AppModelInstance.ValidAudioExtensions, file))
                {
                    e.Handled = false;
                    return;
                }
            }

            e.Handled = true;
        }

        private void AddNewAudio_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                RestoreDirectory = true,
                DefaultExt = "mp3",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true,
                Filter = "Audio Files|" + AudioHandling.FileExtension.GetDialogExtensions(AppModel.Instance.ValidAudioExtensions)
            };
            FileDialog.Title = "Select Object File";
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AudioDropPanelVisibility = Visibility.Visible;
                uc_DropPanel.OnDrop(FileDialog.FileNames.ToList());
            }
        }

        private void StopAllAudio_Click(object sender, RoutedEventArgs e)
        {
            AppModelInstance.AudioManager.StopAllAudio();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppModelInstance.SaveData();
        }




        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanelVisibility = Visibility.Visible;
        }

        private void HideSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanelVisibility = Visibility.Hidden;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
