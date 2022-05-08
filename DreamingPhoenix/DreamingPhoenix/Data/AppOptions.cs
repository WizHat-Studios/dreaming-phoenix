using Newtonsoft.Json;
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
        public event EventHandler OnDefaultOutputDeviceChanged;

        private bool extendedModeEnabled = false;

        public bool ExtendedModeEnabled
        {
            get { return extendedModeEnabled; }
            set { extendedModeEnabled = value; NotifyPropertyChanged(); }
        }

        private double defaultAudioTrackVolume = 0.25;

        public double DefaultAudioTrackVolume
        {
            get { return defaultAudioTrackVolume; }
            set
            {
                if (value > 1)
                    value = 1;

                defaultAudioTrackVolume = value;
                NotifyPropertyChanged();
            }
        }

        private double defaultSoundEffectVolume = 0.25;

        public double DefaultSoundEffectVolume
        {
            get { return defaultSoundEffectVolume; }
            set
            {
                if (value > 1)
                    value = 1;

                defaultSoundEffectVolume = value;
                NotifyPropertyChanged();
            }
        }

        private int defaultOutputDevice = 0;

        public int DefaultOutputDevice
        {
            get { return defaultOutputDevice; }
            set
            {
                defaultOutputDevice = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public int DefaultOutputDeviceIndex
        {
            get { return DefaultOutputDevice + 1; }
            set
            {
                DefaultOutputDevice = value - 1;
                NotifyPropertyChanged();
            }
        }

        private int selectedColorScheme;

        public int SelectedColorScheme
        {
            get { return selectedColorScheme; }
            set
            {
                if (value < 0 || value >= ColorScheme.Themes.Count)
                    value = 0;

                selectedColorScheme = value;
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

        private double fullHeightSceneBackgroundOpacity = 0.4;

        public double FullHeightSceneBackgroundOpacity
        {
            get { return fullHeightSceneBackgroundOpacity; }
            set
            {
                fullHeightSceneBackgroundOpacity = value;
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
            AppModel.Loaded += (s, e) =>
            {
                PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DefaultOutputDevice))
                    {
                        AppModel.Instance.ChangeOutputDevice(AppModel.Instance.Options.DefaultOutputDevice);
                        OnDefaultOutputDeviceChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
            };
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
