using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WizHat.DreamingPhoenix.AudioProperties;

namespace WizHat.DreamingPhoenix.AudioHandling
{
    public abstract class Audio : INotifyPropertyChanged
    {
        private string audioFile;

        /// <summary>
        /// Path to the audio file
        /// </summary>
        public string AudioFile
        {
            get { return audioFile; }
            set
            {
                audioFile = value;
                IsAudioFilePathValid = File.Exists(audioFile);
                NotifyPropertyChanged();
            }
        }

        private bool isAudioFilePathValid;

        [JsonIgnore]
        public bool IsAudioFilePathValid
        {
            get { return isAudioFilePathValid; }
            set { isAudioFilePathValid = value; NotifyPropertyChanged(); }
        }


        private double volume;

        /// <summary>
        /// Volume of the audio file
        /// </summary>
        public double Volume
        {
            get { return volume; }
            set { volume = value; NotifyPropertyChanged(); }
        }

        private bool repeat;

        /// <summary>
        /// If the audio file should repeat after it has finished playing
        /// </summary>
        public bool Repeat
        {
            get { return repeat; }
            set { repeat = value; NotifyPropertyChanged(); }
        }

        private string name;

        /// <summary>
        /// The display name of the audio
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        private Key hotkey;

        /// <summary>
        /// The hotkey to be pressed to trigger the audio to be played
        /// </summary>
        public Key HotKey
        {
            get { return hotkey; }
            set { hotkey = value; NotifyPropertyChanged(); }
        }

        private ModifierKeys hotkeyModifiers;

        /// <summary>
        /// The modifiers of the hotkey that trigger the audio to play
        /// </summary>
        public ModifierKeys HotkeyModifiers
        {
            get { return hotkeyModifiers; }
            set { hotkeyModifiers = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Tag> tags = new ObservableCollection<Tag>();

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set { tags = value; NotifyPropertyChanged(); }
        }

        private Category category = Category.Default;

        public Category Category
        {
            get { return category; }
            set { category = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Creates a new empty Audio
        /// </summary>
        protected Audio()
        {
        }

        /// <summary>
        /// Creates a new Audio
        /// </summary>
        /// <param name="audioFile">Audio File Path</param>
        /// <param name="name">Audio Name</param>
        protected Audio(string audioFile, string name)
        {
            if (!File.Exists(audioFile))
                throw new FileNotFoundException("Audio File not found", audioFile);

            if (audioFile.StartsWith(AppContext.BaseDirectory))
                audioFile = Path.GetRelativePath(AppContext.BaseDirectory, audioFile);

            AudioFile = audioFile;
            Name = name;
        }

        /// <summary>
        /// Updates the IsAudioFilePathValid parameter
        /// </summary>
        public void CheckIfFileExistsOnDisk()
        {
            // Set it to itself, so the setter is triggered and checks if it exists
            AudioFile = AudioFile;
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
