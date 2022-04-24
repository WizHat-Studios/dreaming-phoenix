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
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Cache;
using WizHat.DreamingPhoenix.HotkeyHandling.HotkeySelector;
using WizHat.DreamingPhoenix.HotkeyHandling.KeyboardListener;
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.ExternalAudio;
using System.Collections.Specialized;
using WizHat.DreamingPhoenix.Extensions;
using WizHat.DreamingPhoenix.UserControls;

namespace WizHat.DreamingPhoenix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool pausePlayEnabled = true;
        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        private ObservableStack<DialogControl> dialogStack = new ObservableStack<DialogControl>();
        /// <summary>
        /// Stack containing all active dialogs.
        /// </summary>
        public ObservableStack<DialogControl> DialogStack
        {
            get { return dialogStack; }
            set { dialogStack = value; NotifyPropertyChanged(); }
        }

        public DialogControl topMostDialog = null;
        /// <summary>
        /// Dialog on top of the DialogStack
        /// </summary>
        public DialogControl TopMostDialog
        {
            get { return topMostDialog; }
            set { topMostDialog = value; NotifyPropertyChanged(); }
        }

        private Visibility dialogPanelVisibility = Visibility.Collapsed;
        public Visibility DialogPanelVisibility
        {
            get { return dialogPanelVisibility; }
            set { dialogPanelVisibility = value; NotifyPropertyChanged(); }
        }


        KeyboardHook HotKeyHook = new WizHat.DreamingPhoenix.HotkeyHandling.KeyboardListener.KeyboardHook();

        public MainWindow()
        {
            InitializeComponent();

            DialogStack.CollectionChanged += DialogStack_CollectionChanged;
            HotKeyHook.OnKeyboard += HotKeyHook_OnKeyboard;
            DialogPanelVisibility = Visibility.Collapsed;

            // Cast lbox_audioList to get the collection changed.
            // We need to do this as we cannot use the Collection Changed of the AppModel ObservableCollectio due it being reset many times at runtime.
            ((INotifyCollectionChanged)lbox_audioList.Items).CollectionChanged += DetermineAudioListPrompt;
            ((INotifyCollectionChanged)lbox_sceneList.Items).CollectionChanged += DetermineSceneListPrompt;

            this.DataContext = this;
            SubscribeToAudioTrack();
        }

        private async void DialogStack_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DialogStack.Count > 0)
            {
                TopMostDialog = DialogStack.Peek();
            }
            else
            {
                await Task.Delay(200);
                TopMostDialog = null;
            }
        }

        private void DetermineSceneListPrompt(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AppModelInstance.SceneList.Count == 0)
            {
                grid_emptySceneListboxPrompt.Visibility = Visibility.Visible;
            }
            else
            {
                grid_emptySceneListboxPrompt.Visibility = Visibility.Collapsed;
            }
        }

        private void DetermineAudioListPrompt(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (AppModelInstance.SearchResultAudioList.Count == 0)
            {
                if (AppModelInstance.AudioList.Count == 0)
                {
                    grid_emptyListboxPrompt.Visibility = Visibility.Visible;
                    grid_noSearchListboxPrompt.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_emptyListboxPrompt.Visibility = Visibility.Collapsed;
                    grid_noSearchListboxPrompt.Visibility = Visibility.Visible;
                }
            }
            else
            {
                grid_emptyListboxPrompt.Visibility = Visibility.Collapsed;
                grid_noSearchListboxPrompt.Visibility = Visibility.Collapsed;
            }
        }

        private void HotKeyHook_OnKeyboard(object sender, WizHat.DreamingPhoenix.HotkeyHandling.KeyboardListener.KeyboardEventArgs e)
        {
            if (e.KeyState != KeyState.WM_KEYUP || HotkeySelector.GlobalIsInSelectionModeLock)
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
            AppModel.Instance.AudioManager.StopAudio((PlayableAudio)((Button)sender).DataContext, !AppModelInstance.Options.FadeSoundEffectsOnStop, AppModelInstance.Options.FadeSoundEffectsOnStop);
        }

        private async void RemoveAudio_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;

            Audio audioToDelete = (Audio)((Button)sender).DataContext;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                new AudioDeletion(audioToDelete).DeleteWithoutConfirmation();
            }
            else
            {
                await ShowDialog(new AudioDeletion(audioToDelete));
            }
            grid_selectedAudioProperties.Children.Clear();
        }

        private void AudioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            SetPropertiesPanelFromAudio((Audio)((ListBox)sender).SelectedItem);
        }

        private void AudioListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            SetPropertiesPanelFromAudio((Audio)((ListBox)sender).SelectedItem);
        }

        private void AudioListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AppModel.Instance.AudioManager.PlayAudio((Audio)((ListBoxItem)sender).DataContext);
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

        private void SetPropertiesPanelFromObject(object obj)
        {
            switch (obj)
            {
                case Audio a:
                    SetPropertiesPanelFromAudio(a);
                    break;
                case Scene s:
                    grid_selectedAudioProperties.Children.Clear();
                    grid_selectedAudioProperties.Children.Add(new UserControls.SceneProperties(s));
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
            List<string> files = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();

            
            this.Activate();

            ShowDialog(new FileDragDrop(files));
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            bool allowDrop = false;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files == null)
                return;

            foreach (string file in files)
            {
                if (FileExtension.EndsWith(AppModelInstance.ValidAudioExtensions, file) || FileExtension.EndsWith(AppModelInstance.ValidScenePackageExtensions, file))
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
                Filter = "Audio Files|" + FileExtension.GetDialogExtensions(AppModel.Instance.ValidAudioExtensions)
            };
            FileDialog.Title = "Select Object File";
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                ShowDialog(new FileDragDrop(FileDialog.FileNames.ToList()));
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
            CacheManager.Instance.CleanUpCache();
        }

        private async void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            await ShowDialog(new UserApplicationOptions());
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

            if ((SoundEffect)((PlayableAudio)((ListBox)sender).SelectedItem).CurrentAudio == SoundEffect.Default)
                return;

            SetPropertiesPanelFromAudio((SoundEffect)((PlayableAudio)((ListBox)sender).SelectedItem).CurrentAudio);
        }

        private void SoundEffectsListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;

            if ((SoundEffect)((PlayableAudio)((ListBox)sender).SelectedItem).CurrentAudio == SoundEffect.Default)
                return;

            SetPropertiesPanelFromAudio((SoundEffect)((PlayableAudio)((ListBox)sender).SelectedItem).CurrentAudio);
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            AppModelInstance.Options.FilterOptions.SearchTerm = "";
        }

        private async void btn_filterSettings_Click(object sender, RoutedEventArgs e)
        {
            await ShowDialog(new FilterSettings());
            await AppModelInstance.ApplyFilterOptions(AppModelInstance.Options.FilterOptions);
        }

        private async void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            await AppModelInstance.ApplyFilterOptions(AppModelInstance.Options.FilterOptions);
        }

        private async void window_MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await AppModelInstance.ApplyFilterOptions(AppModelInstance.Options.FilterOptions);

            foreach (Scene scene in AppModelInstance.SceneList)
            {
                scene.ImageSource = CacheManager.Instance.GetImageFromCache(scene.ImageCacheID);
            }
        }

        private void AudioTrackNameDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (AppModelInstance.AudioManager.CurrentlyPlayingAudioTrack == null)
                return;

            if (AppModelInstance.AudioManager.CurrentlyPlayingAudioTrack.CurrentAudio == AudioTrack.Default)
                return;

            SetPropertiesPanelFromAudio(AppModelInstance.AudioManager.CurrentlyPlayingAudioTrack.CurrentAudio);
        }

        private void SceneListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            SetPropertiesPanelFromObject(((Scene)((ListBox)sender).SelectedItem));
        }

        private void SceneListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            SetPropertiesPanelFromObject(((Scene)((ListBox)sender).SelectedItem));
        }

        private void PlayScene_Click(object sender, RoutedEventArgs e)
        {
            AppModelInstance.AudioManager.PlayScene((sender as Button).Tag as Scene);
        }

        private void RemoveScene_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;

            ShowDialog(new SceneDeletion((Scene)((Button)sender).DataContext)).Wait();

            grid_selectedAudioProperties.Children.Clear();
        }

        private void btn_addNewScene_Click(object sender, RoutedEventArgs e)
        {
            Scene newScene = new Scene() { SceneName = "New Scene" };
            AppModelInstance.SceneList.Add(newScene);
            tabcontrol_main.SelectedIndex = 1;
            lbox_sceneList.SelectedItem = newScene;
            lbox_sceneList.Focus();
        }

        private void btn_addYouTube_Click(object sender, RoutedEventArgs e)
        {
            if (!YouTubeAudio.FFMPEGExists())
            {
                string downloadLink = "https://ffbinaries.com/downloads";
                string text = $"To download audio from youtube, you have to download ffmpeg. \r\nDownload link: {downloadLink} \r\nPlease place the downloaded exe file in the same directory as Dreaming Phoenix";
                MessageBoxResult result = MessageBox.Show(text, "ffmpeg.exe missing", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadLink,
                        UseShellExecute = true
                    });
                }
                return;
            }

            ShowDialog(new YouTubeDownloader());
        }

        /// <summary>
        /// Shows a new dialog and awaits its closing with a return value.
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="dialog">Dialog to display</param>
        /// <returns>The return value of type T</returns>
        public async Task<T> ShowDialog<T>(DialogControl dialog)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            dialog.Owner = this;
            dialog.DialogClosed += (s, e) =>
            {
                tcs.SetResult((T)e.ReturnValue);
                DialogPanelVisibility = Visibility.Collapsed;
            };
            DialogStack.Push(dialog);
            DialogPanelVisibility = Visibility.Visible;

            return await tcs.Task;
        }

        /// <summary>
        /// Shows a new dialog in the current window and awaits its closing.
        /// </summary>
        /// <param name="dialog">The dialog to display.</param>
        /// <returns>void</returns>
        public async Task ShowDialog(DialogControl dialog)
        {
            await ShowDialog<object>(dialog);
        }

        private void window_MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.Height = AppModelInstance.WindowOptions.Height;
            this.Width = AppModelInstance.WindowOptions.Width;
        }

        private void DialogBackground_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TopMostDialog.AllowExitOnBackgroundClick)
            {
                TopMostDialog.Close();
            }
        }
    }
}
