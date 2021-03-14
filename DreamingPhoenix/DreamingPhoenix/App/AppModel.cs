﻿using DreamingPhoenix.AudioHandling;
using DreamingPhoenix.Styles.Scheme;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace DreamingPhoenix
{
    public class AppModel : INotifyPropertyChanged
    {

        private static AppModel instance;

        /// <summary>
        /// Instance of the Singleton Implementation
        /// </summary>
        public static AppModel Instance
        {
            get 
            {
                if (instance == null)
                    instance = new AppModel();

                return instance;
            }
        }

        private string searchTerm = "";

        /// <summary>
        /// String used by the searchResultAudioList to filter the audio results
        /// </summary>
        public string SearchTerm
        {
            get { return searchTerm; }
            set { searchTerm = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(SearchActive)); NotifyPropertyChanged(nameof(SearchResultAudioList)); }
        }

        public bool SearchActive { get { return !String.IsNullOrEmpty(SearchTerm); } }


        /// <summary>
        /// List containing all audio which match with the search term
        /// </summary>
        public ObservableCollection<Audio> SearchResultAudioList
        {
            get 
            {
                return new ObservableCollection<Audio>(AudioList.Where(x => x.Name.ToLower().Contains(SearchTerm.ToLower())).ToList()); 
            }
        }


        private ObservableCollection<Audio> audioList = new ObservableCollection<Audio>();

        /// <summary>
        /// List containing all audio configurations with tracks and effects
        /// </summary>
        public ObservableCollection<Audio> AudioList
        {
            get { return audioList; }
            set { audioList = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(SearchResultAudioList)); }
        }


        private AppOptions options = new AppOptions();

        /// <summary>
        /// Options of the application
        /// </summary>
        public AppOptions Options
        {
            get { return options; }
            set { options = value; NotifyPropertyChanged(); }
        }

        public AudioManager AudioManager { get; set; } = new AudioManager();

        public List<FileExtension> ValidAudioExtensions = new List<FileExtension>()
        {
            new FileExtension("wav"),
            new FileExtension("aiff"),
            new FileExtension("mp3"),
            new FileExtension("wma"),
            new FileExtension("aac")
        };

        public AppModel()
        {
            LoadData();
            AudioList.CollectionChanged += (s, e) => { NotifyPropertyChanged(nameof(SearchResultAudioList)); };
        }

        public void SaveData()
        {
            Persistence.PersistentData persistentData = new Persistence.PersistentData()
            {
                AudioList = new List<Audio>(AudioList),
                AppOptions = Options
            };

            new Persistence.PersistenceJsonDataManager().Save(persistentData);
        }

        public void LoadData()
        {
            Persistence.PersistentData data = new Persistence.PersistenceJsonDataManager().Load();
            AudioList.Clear();

            if (data != null)
            {
                data.AudioList.ForEach(x => AudioList.Add(x));
                Options = data.AppOptions;

                if (Options.DefaultOutputDevice - 1 > WaveOut.DeviceCount - 1)
                    Options.DefaultOutputDevice = -1;
            }
        }

        /// <summary>
        /// Change the NAudio output device
        /// </summary>
        /// <param name="outputDevice">Output Device number</param>
        public void ChangeOutputDevice(int outputDevice)
        {
            if (AudioManager.OutputDevice == null || AudioManager.MixingProvider == null)
                return;
            AudioManager.OutputDevice.Stop();
            AudioManager.OutputDevice = new WaveOutEvent() { DeviceNumber = outputDevice };
            AudioManager.OutputDevice.Init(AudioManager.MixingProvider);
            AudioManager.OutputDevice.Play();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
