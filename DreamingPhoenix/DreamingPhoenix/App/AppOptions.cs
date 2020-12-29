using DreamingPhoenix.Styles.Scheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

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

        private int selectedColorScheme;

        public int SelectedColorScheme
        {
            get { return selectedColorScheme; }
            set
            { 
                selectedColorScheme = value; 
                if (value >= 0 && value < ColorScheme.Themes.Count)
                    ((App)Application.Current).ChangeTheme(ColorScheme.Themes[value].ThemeDestination); 
                NotifyPropertyChanged();
            }
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
