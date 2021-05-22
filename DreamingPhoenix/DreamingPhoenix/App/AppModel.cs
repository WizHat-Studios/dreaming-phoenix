using DreamingPhoenix.AudioHandling;
using DreamingPhoenix.Styles.Scheme;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

        public bool SearchActive { get { return !String.IsNullOrEmpty(Options.FilterOptions.SearchTerm); } }


        private ObservableCollection<Audio> searchResultAudioList;
        /// <summary>
        /// List containing all audio which match with the search term
        /// </summary>
        public ObservableCollection<Audio> SearchResultAudioList
        {
            get 
            {
                return searchResultAudioList;
            }
            private set
            {
                searchResultAudioList = value; NotifyPropertyChanged();
            }
        }

        public async Task ApplyFilterOptions(FilterOptions filterOtions)
        {
            NotifyPropertyChanged("SearchActive");

            await Task.Run(() =>
            {

                ObservableCollection<Audio> filteredList = new ObservableCollection<Audio>();
                foreach (var audio in AudioList)
                {
                    if (!audio.Name.ToLower().Contains(filterOtions.SearchTerm.ToLower()) && !String.IsNullOrWhiteSpace(filterOtions.SearchTerm))
                        continue;

                    if (!filterOtions.IncludeAudioTracks && audio is AudioTrack)
                        continue;

                    if (!filterOtions.IncludeSoundEffects && audio is SoundEffect)
                        continue;

                    filteredList.Add(audio);
                }

                if (filterOtions.SortDirection == SortDirection.ASCENDING)
                    SearchResultAudioList = new ObservableCollection<Audio>(filteredList.OrderBy(x => x.Name).ToList());
                else
                    SearchResultAudioList = new ObservableCollection<Audio>(filteredList.OrderByDescending(x => x.Name).ToList());
            });
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
            searchResultAudioList = audioList;
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
