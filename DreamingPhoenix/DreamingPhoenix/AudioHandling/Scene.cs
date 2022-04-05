using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace DreamingPhoenix.AudioHandling
{
    [DebuggerDisplay("(Scene) {SceneName,np} - AudioTrack: {SceneAudioTrack.Name,np}, SoundEffectsCount: {SceneSoundEffects.Count,np}")]
    public class Scene : INotifyPropertyChanged
    {
        private ObservableCollection<SoundEffect> sceneSoundEffects = new ObservableCollection<SoundEffect>();

        /// <summary>
        /// Sound Effects of the Scene
        /// </summary>
        public ObservableCollection<SoundEffect> SceneSoundEffects
        {
            get { return sceneSoundEffects; }
            set { sceneSoundEffects = value; NotifyPropertyChanged(); }
        }

        private AudioTrack sceneAudioTrack = null;

        /// <summary>
        /// Audio Track of the Scene
        /// </summary>
        public AudioTrack SceneAudioTrack
        {
            get { return sceneAudioTrack; }
            set { sceneAudioTrack = value; NotifyPropertyChanged(); }
        }

        private string sceneName;

        /// <summary>
        /// Name of the Scene
        /// </summary>
        public string SceneName
        {
            get { return sceneName; }
            set { sceneName = value; NotifyPropertyChanged(); }
        }


        private ImageSource imageSource;

        // Ignore as it does not have to be saved
        [JsonIgnore]
        public ImageSource ImageSource
        {
            get { return imageSource; }
            set { imageSource = value; NotifyPropertyChanged(); }
        }

        private string imageCacheID;

        public string ImageCacheID
        {
            get { return imageCacheID; }
            set { imageCacheID = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
