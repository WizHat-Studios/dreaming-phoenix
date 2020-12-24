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

        public AudioHandling.AudioTrack a1 { get; set; } = new AudioHandling.AudioTrack(@"D:\WizHat Studios\Sounds\Music1.mp3", "Jackpot"); 
        public AudioHandling.AudioTrack a2;
        public AudioHandling.SoundEffect s1;

        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // a1 =;
            a1.Volume = 0.1f;
            a1.FadeOutSpeed = 5000;

            a2 = new AudioHandling.AudioTrack(@"D:\WizHat Studios\Sounds\Music2.mp3", "Feels");
            a2.Volume = 0.1f;
            a2.FadeOutSpeed = 5000;

            s1 = new AudioHandling.SoundEffect(@"D:\WizHat Studios\Sounds\Sound1.mp3", "Puzzle");

            AppModelInstance.AudioList.Add(a1);
            AppModelInstance.AudioList.Add(a2);

            // AppModelInstance.AudioManager.CurrentlyPlayingAudioTrack = new AudioHandling.PlayableAudio(a1);

            AppModelInstance.AudioManager.CurrentlyPlayingSoundEffects.Add(new AudioHandling.PlayableAudio(s1));
            AppModelInstance.AudioManager.CurrentlyPlayingSoundEffects.Add(new AudioHandling.PlayableAudio(s1));
            AppModelInstance.AudioManager.CurrentlyPlayingSoundEffects.Add(new AudioHandling.PlayableAudio(s1));
            // AppModelInstance.AudioList.Add(new AudioHandling.SoundEffect(@"D:\Music\Soundboard\Files\badumtss.swf.mp3", "Badummmts"));

            grid_selectedAudioProperties.Children.Clear();
            grid_selectedAudioProperties.Children.Add(new UserControls.AudioTrackProperties(a1));
        }

        private void PlayAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext == null)
                return;
            AudioHandling.AudioTrack track = (AudioHandling.AudioTrack)((Button)sender).DataContext;
            AppModel.Instance.AudioManager.PlayAudio(track);
        }
    }
}
