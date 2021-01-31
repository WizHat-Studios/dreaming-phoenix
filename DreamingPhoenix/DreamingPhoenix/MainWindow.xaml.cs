using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private bool pausePlayEnabled = true;
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

        HotkeyHandling.KeyboardListener.KeyboardHook HotKeyHook = new HotkeyHandling.KeyboardListener.KeyboardHook();

        public MainWindow()
        {
            InitializeComponent();

            grid_AppOptions.Visibility = Visibility.Visible;
            grid_DropPanel.Visibility = Visibility.Visible;
            HotKeyHook.OnKeyboard += HotKeyHook_OnKeyboard;

            uc_DropPanel.AudioFilesProcessed += (s, e) => { AudioDropPanelVisibility = Visibility.Hidden; AppModelInstance.SaveData(); };

            this.DataContext = this;
            SubscribeToAudioTrack();
        }

        private void HotKeyHook_OnKeyboard(object sender, HotkeyHandling.KeyboardListener.KeyboardEventArgs e)
        {
            if (e.KeyState != HotkeyHandling.KeyboardListener.KeyState.WM_KEYUP || HotkeyHandling.HotkeySelector.HotkeySelector.GlobalIsInSelectionModeLock)
                return;

            foreach (Audio audio in AppModelInstance.AudioList)
            {
                if (audio.HotKey == e.Key && Keyboard.Modifiers == audio.HotkeyModifiers)
                {
                    AppModelInstance.AudioManager.PlayAudio(audio);
                    e.Handled = true;
                }
            }

            if (AppModelInstance.Options.StopAllAudioHotKey == e.Key && Keyboard.Modifiers == AppModelInstance.Options.StopAllAudioHotKeyModifier)
            {
                AppModelInstance.AudioManager.StopAllAudio();
            }
        }

        private void PlayAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            AppModel.Instance.AudioManager.PlayAudio((AudioTrack)((Button)sender).DataContext);
        }

        private async void PlayPauseAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            if (pausePlayEnabled)
            {
                pausePlayEnabled = false;
                await AppModel.Instance.AudioManager.PausePlayAudio();
            }
        }

        private void btn_PlayNextAudio_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.AudioManager.PlayNextTrack();
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
            AppModel.Instance.AudioManager.StopAudio((PlayableAudio)((Button)sender).DataContext, true);
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
            SetPropertiesPanelFromAudio(((Audio)((ListBox)sender).SelectedItem));
        }

        private void AudioListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            SetPropertiesPanelFromAudio(((Audio)((ListBox)sender).SelectedItem));
        }

        private void SetPropertiesPanelFromAudio(Audio audio)
        {
            switch (audio)
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

            AudioDropPanelVisibility = Visibility.Visible;
            this.Activate();
            uc_DropPanel.OnDrop(files.ToList());
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            bool allowDrop = false;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                if (FileExtension.EndsWith(AppModelInstance.ValidAudioExtensions, file))
                {
                    allowDrop = true;
                    break;
                }
            }

            if (!allowDrop)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
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
            SubscribeToAudioTrack();
        }

        private void SubscribeToAudioTrack()
        {
            AppModel.Instance.AudioManager.CurrentlyPlayingAudioTrack.AudioStopped += (s, e) =>
            {
                btn_PauseAudioTrack.Dispatcher.Invoke(() =>
                {
                    btn_PauseAudioTrack.Visibility = Visibility.Collapsed;
                    btn_PlayAudioTrack.Visibility = Visibility.Visible;
                    pgb_audioTrack.Value = 0;
                    pgb_audioTrack.Maximum = 0;
                    btn_PlayAudioTrack.IsEnabled = false;
                    pausePlayEnabled = true;
                });
            };
            AppModel.Instance.AudioManager.CurrentlyPlayingAudioTrack.AudioTrackTick += (currSecond, totalSeconds) =>
            {
                pgb_audioTrack.Dispatcher.Invoke(() =>
                {
                    pgb_audioTrack.Value = currSecond;
                    pgb_audioTrack.Maximum = totalSeconds;
                });
            };
            AppModel.Instance.AudioManager.CurrentlyPlayingAudioTrack.AudioStarted += (s, e) =>
            {
                btn_PauseAudioTrack.Dispatcher.Invoke(() =>
                {
                    btn_PauseAudioTrack.Visibility = Visibility.Visible;
                    btn_PlayAudioTrack.Visibility = Visibility.Collapsed;
                    btn_PlayAudioTrack.IsEnabled = true;
                    pausePlayEnabled = true;
                });
            };
            AppModel.Instance.AudioManager.CurrentlyPlayingAudioTrack.AudioPaused += (s, e) =>
            {
                btn_PauseAudioTrack.Dispatcher.Invoke(() =>
                {
                    btn_PauseAudioTrack.Visibility = Visibility.Collapsed;
                    btn_PlayAudioTrack.Visibility = Visibility.Visible;
                    pausePlayEnabled = true;
                });
            };
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
            AppModelInstance.SaveData();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SoundEffectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;

            SetPropertiesPanelFromAudio((SoundEffect)((PlayableAudio)((ListBox)sender).SelectedItem).CurrentAudio);
        }

        private void SoundEffectsListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;

            SetPropertiesPanelFromAudio((SoundEffect)((PlayableAudio)((ListBox)sender).SelectedItem).CurrentAudio);
        }
    }
}
