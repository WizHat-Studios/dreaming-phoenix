using WizHat.DreamingPhoenix.Styles.Scheme;
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
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.AudioProperties;
using WizHat.DreamingPhoenix.Sorting;
using System.Diagnostics;

namespace WizHat.DreamingPhoenix.Data
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

        public static event EventHandler Loaded;

        private ObservableCollection<Audio> audioList = new ObservableCollection<Audio>();

        /// <summary>
        /// List containing all audio configurations with tracks and effects
        /// </summary>
        public ObservableCollection<Audio> AudioList
        {
            get { return audioList; }
            set { audioList = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Scene> sceneList = new ObservableCollection<Scene>();

        public ObservableCollection<Scene> SceneList
        {
            get { return sceneList; }
            set { sceneList = value; NotifyPropertyChanged(); }
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

        private WindowOptions windowOptions = new WindowOptions();

        public WindowOptions WindowOptions
        {
            get { return windowOptions; }
            set { windowOptions= value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Tag> availableTags = new ObservableCollection<Tag>();

        public ObservableCollection<Tag> AvailableTags
        {
            get { return availableTags; }
            set { availableTags = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Category> categories = new();

        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set { categories = value; NotifyPropertyChanged(); }
        }

        public void UpdateAvailableTags()
        {
            ObservableCollection<Tag> newTags = new ObservableCollection<Tag>();

            foreach (Audio audio in AudioList)
            {
                foreach (Tag tag in audio.Tags)
                {
                    if (!newTags.Contains(tag))
                    {
                        newTags.Add(tag);
                    }
                }
            }

            AvailableTags = newTags;
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

        public List<FileExtension> ValidScenePackageExtensions = new List<FileExtension>()
        {
            new FileExtension("zip")
        };

        public AppModel()
        {
            LoadData();
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        public void SaveData()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.WindowState == WindowState.Normal)
            {
                WindowOptions.Height = mainWindow.Height;
                WindowOptions.Width = mainWindow.Width;
            }

            Persistence.PersistentData persistentData = new Persistence.PersistentData()
            {
                Categories = new(Categories),
                AudioList = new(AudioList),
                SceneList = new(SceneList),
                AppOptions = Options,
                WindowOptions = WindowOptions
            };

            new Persistence.PersistenceJsonDataManager().Save(persistentData);
        }

        public void LoadData()
        {
            Persistence.PersistentData data = new Persistence.PersistenceJsonDataManager().Load();
            AudioList.Clear();

            if (data != null)
            {
                data.Categories.ForEach(c => Categories.Add(c));
                data.AudioList.ForEach(a => AudioList.Add(a));
                data.SceneList.ForEach(s => SceneList.Add(s));
                Options = data.AppOptions;
                WindowOptions = data.WindowOptions;

                if (Options.DefaultOutputDevice > WaveOut.DeviceCount - 1)
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

        /// <summary>
        /// Add all non existing categories from a list of audios
        /// </summary>
        /// <param name="audios">The list of songs from which the category should be added</param>
        public void AddCategories(List<Audio> audios)
        {
            throw new NotImplementedException();

            foreach (Audio audio in audios)
            {
                Debug.WriteLine($"Does category exist: {Categories.Contains(audio.Category)}");
            }
        }

        /// <summary>
        /// Remove a category globally (Includes all audios)
        /// </summary>
        /// <param name="category">The category to remove</param>
        public void RemoveCategory(Category category)
        {
            AudioList.Where(a => a.Category.Equals(category)).ToList().ForEach(a => a.Category = Category.Default);
            Categories.Remove(category);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
