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
    /// Interaction logic for AudioTrackProperties.xaml
    /// </summary>
    public partial class AudioTrackProperties : UserControl, INotifyPropertyChanged
    {
        public AudioTrack Track { get; set; }
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

        public AudioTrackProperties(AudioTrack audioTrack)
        {
            InitializeComponent();
            this.DataContext = this;
            Track = audioTrack;
            Tracks = AppModel.Instance.AudioList.Where(a => a.GetType() == typeof(AudioTrack) && a != Track).ToList();
        }

        private void cmb_nextAudioTrack_Loaded(object sender, RoutedEventArgs e)
        {
            cmb_nextAudioTrack.SelectedItem = Track.NextAudioTrack;
        }

        private void cmb_nextAudioTrack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmb_nextAudioTrack.SelectedIndex == -1)
                return;

            Track.NextAudioTrack = (AudioTrack)cmb_nextAudioTrack.SelectedItem;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Track == null)
                return;

            AppModel.Instance.AudioManager.PlayAudio(Track);
        }

        private void RemoveNextTrack_Click(object sender, RoutedEventArgs e)
        {
            Track.NextAudioTrack = null;
            cmb_nextAudioTrack.SelectedItem = null;
        }

        private void DeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.AudioList.Remove(Track);
        }
    }
}
