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
using System.Windows.Media;

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

        private ObservableCollection<Tag> tags = new ObservableCollection<Tag>();

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set { tags = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Category> categories = new();

        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set { categories = value; NotifyPropertyChanged(); }
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
                Tags = new(Tags),
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
                data.Tags.ForEach(t => Tags.Add(t));
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

        #region Category
        /// <summary>
        /// Add all non existing categories from a list of audios
        /// </summary>
        /// <param name="audios">The list of songs from which the category should be added</param>
        public void AddCategoryFromAudio(List<Audio> audios)
        {
            foreach (Audio audio in audios.Where(a => !a.Category.IsDefault()))
            {
                if (!Categories.Contains(audio.Category))
                    Categories.Add(audio.Category);
            }
        }

        /// <summary>
        /// Add the category from a audio if it does not exist
        /// </summary>
        /// <param name="fromAudio">The audio from which the category should be added</param>
        public void AddCategoryFromAudio(Audio fromAudio)
        {
            AddCategoryFromAudio(new List<Audio>() { fromAudio });
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

        public void ChangeCategoryColor(Category category, Color newColor)
        {
            if (!Categories.Contains(category))
                return;

            Categories.First(c => c.Equals(category)).Color = newColor;
        }
        #endregion

        #region Tag
        /// <summary>
        /// Add all non existing tags from a list of audios
        /// </summary>
        /// <param name="audios">The list of songs from which the tags should be added</param>
        public void AddTagsFromAudio(List<Audio> audios)
        {
            foreach (Audio audio in audios.Where(a => a.Tags.Count > 0))
            {
                foreach (Tag tag in audio.Tags)
                {
                    if (!Tags.Contains(tag))
                        Tags.Add(tag);
                }
            }
        }

        /// <summary>
        /// Add the tags from a audio if it does not exist
        /// </summary>
        /// <param name="fromAudio">The audio from which the tags should be added</param>
        public void AddTagsFromAudio(Audio fromAudio)
        {
            AddTagsFromAudio(new List<Audio>() { fromAudio });
        }

        /// <summary>
        /// Remove a tag globally (Includes all audios)
        /// </summary>
        /// <param name="tag">The tag to remove</param>
        public void RemoveTag(Tag tag)
        {
            foreach (Audio audio in AudioList)
            {
                audio.Tags.Remove(tag);
            }

            Tags.Remove(tag);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
