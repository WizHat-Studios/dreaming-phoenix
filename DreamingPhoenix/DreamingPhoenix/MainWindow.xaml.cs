using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DreamingPhoenix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void PlayAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            AudioHandling.AudioTrack track = (AudioHandling.AudioTrack)((Button)sender).DataContext;
            AppModel.Instance.AudioManager.PlayAudio(track);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppModelInstance.SaveData();
        }

        private void AudioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;

            switch ((AudioHandling.Audio)((ListBox)sender).SelectedItem)
            {
                case AudioHandling.AudioTrack at:
                    grid_selectedAudioProperties.Children.Clear();
                    grid_selectedAudioProperties.Children.Add(new UserControls.AudioTrackProperties(at));
                    break;
                case AudioHandling.SoundEffect se:
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
    }
}
