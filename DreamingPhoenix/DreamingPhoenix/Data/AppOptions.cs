using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WizHat.DreamingPhoenix.Styles.Scheme;

namespace WizHat.DreamingPhoenix.Data
{
    public class AppOptions : INotifyPropertyChanged
    {

        private bool extendedModeEnabled = false;

        public bool ExtendedModeEnabled
        {
            get { return extendedModeEnabled; }
            set { extendedModeEnabled = value; NotifyPropertyChanged(); }
        }

        private float defaultAudioTrackVolume = 0.25f;

        public float DefaultAudioTrackVolume
        {
            get { return defaultAudioTrackVolume; }
            set { defaultAudioTrackVolume = value; NotifyPropertyChanged(); }
        }

        private float defaultSoundEffectVolume = 0.25f;

        public float DefaultSoundEffectVolume
        {
            get { return defaultSoundEffectVolume; }
            set { defaultSoundEffectVolume = value; NotifyPropertyChanged(); }
        }

        private int defaultOutputDevice = 0;

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

        private bool fadeAudioOnPause = false;

        public bool FadeAudioOnPause
        {
            get { return fadeAudioOnPause; }
            set
            {
                fadeAudioOnPause = value;
                NotifyPropertyChanged();
            }
        }

        private bool fadeSoundEffectsOnStop = false;

        public bool FadeSoundEffectsOnStop
        {
            get { return fadeSoundEffectsOnStop; }
            set
            {
                fadeSoundEffectsOnStop = value;
                NotifyPropertyChanged();
            }
        }

        private bool useFullHeightForSceneBackground = false;

        public bool UseFullHeightForSceneBackground
        {
            get { return useFullHeightForSceneBackground; }
            set
            {
                useFullHeightForSceneBackground = value;
                NotifyPropertyChanged();
            }
        }

        private FilterOptions filterOptions;

        public FilterOptions FilterOptions
        {
            get
            {
                if (filterOptions == null)
                {
                    filterOptions = new FilterOptions();
                }
                return filterOptions;
            }
            set { filterOptions = value; NotifyPropertyChanged(); }
        }


        private Key stopAllAudioHotKey;

        public Key StopAllAudioHotKey
        {
            get { return stopAllAudioHotKey; }
            set { stopAllAudioHotKey = value; NotifyPropertyChanged(); }
        }

        private ModifierKeys stopAllAudioHotKeyModifier;

        public ModifierKeys StopAllAudioHotKeyModifier
        {
            get { return stopAllAudioHotKeyModifier; }
            set { stopAllAudioHotKeyModifier = value; NotifyPropertyChanged(); }
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
