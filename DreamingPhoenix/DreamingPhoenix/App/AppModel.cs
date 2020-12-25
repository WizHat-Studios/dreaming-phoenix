using DreamingPhoenix.AudioHandling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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

        private ObservableCollection<Audio> audioList = new ObservableCollection<Audio>();

        /// <summary>
        /// List containing all audio configurations with tracks and effects
        /// </summary>
        public ObservableCollection<Audio> AudioList
        {
            get { return audioList; }
            set { audioList = value; NotifyPropertyChanged(); }
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

        public void SaveData()
        {
            Persistence.PersistentData persistentData = new Persistence.PersistentData() { AudioList = new List<Audio>(AudioList) };
            new Persistence.PersistenceJsonDataManager().Save(persistentData);
        }

        public void LoadData()
        {
            Persistence.PersistentData data = new Persistence.PersistenceJsonDataManager().Load();
            AudioList.Clear();
            data.AudioList.ForEach(x => AudioList.Add(x));
        }

        public AppModel()
        {
            LoadData();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
