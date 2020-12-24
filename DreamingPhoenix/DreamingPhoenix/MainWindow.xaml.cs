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

        public AudioHandling.AudioTrack a1 { get; set; } = new AudioHandling.AudioTrack(@"D:\Music\Soundboard\Files\Directed by Robert B. Weide- theme meme.mp3", "Theme"); 
        public AudioHandling.AudioTrack a2;

        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // a1 =;
            a1.Volume = 0.1f;
            a1.FadeOutSpeed = 5000;

            a2 = new AudioHandling.AudioTrack(@"D:\Music\Soundboard\Files\Discord Call Ringtone Remix - Discord Call Ringtone Remix.mp3", "Discord");
            a2.Volume = 0.1f;
            a2.FadeOutSpeed = 5000;

            AppModelInstance.AudioList.Add(a1);
            AppModelInstance.AudioList.Add(a2);
            // AppModelInstance.AudioList.Add(new AudioHandling.SoundEffect(@"D:\Music\Soundboard\Files\badumtss.swf.mp3", "Badummmts"));

            grid_selectedAudioProperties.Children.Clear();
            grid_selectedAudioProperties.Children.Add(new UserControls.AudioTrackProperties(a1));
        }
    }
}
