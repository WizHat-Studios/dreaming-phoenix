using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DreamingPhoenix
{
    public class AppOptions : INotifyPropertyChanged
    {

        private bool extendedModeEnabled = false;

        public bool ExtendedModeEnabled
        {
            get { return extendedModeEnabled; }
            set { extendedModeEnabled = value; NotifyPropertyChanged(); }
        }

        private float defaultAudioTrackVolume;

        public float DefaultAudioTrackVolume
        {
            get { return defaultAudioTrackVolume; }
            set { defaultAudioTrackVolume = value; NotifyPropertyChanged(); }
        }

        private float defaultSoundEffectVolume;

        public float DefaultSoundEffectVolume
        {
            get { return defaultSoundEffectVolume; }
            set { defaultSoundEffectVolume = value; NotifyPropertyChanged(); }
        }

        private int defaultOutputDevice = -1;

        public int DefaultOutputDevice
        {
            get { return defaultOutputDevice; }
            set { defaultOutputDevice = value; NotifyPropertyChanged(); }
        }


        public AppOptions()
        {

        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
