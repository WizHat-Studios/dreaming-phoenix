using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    public class AudioManager : INotifyPropertyChanged
    {

        private ObservableCollection<PlayableAudio> currentlyPlayingSoundEffects;

        /// <summary>
        /// List of all currently playing sound effects
        /// </summary>
        public ObservableCollection<PlayableAudio> CurrentlyPlayingSoundEffects
        {
            get { return currentlyPlayingSoundEffects; }
            set { currentlyPlayingSoundEffects = value; NotifyPropertyChanged(); }
        }

        private PlayableAudio currentlyPlayingAudioTrack;

        /// <summary>
        /// The audio track which is currently played by the application
        /// </summary>
        public PlayableAudio CurrentlyPlayingAudioTrack
        {
            get { return currentlyPlayingAudioTrack; }
            set { currentlyPlayingAudioTrack = value; NotifyPropertyChanged(); }
        }

        public void PlayAudio(Audio audioToPlay)
        {
            switch (audioToPlay)
            {
                case AudioTrack at:
                    PlayAudioTrack(at);
                    break;
                case SoundEffect se:
                    PlaySoundEffect(se);
                    break;
                default:
                    throw new NotSupportedException("The given type is not supported for being played by the AudioManager");
            }
        }

        private void PlaySoundEffect(SoundEffect soundEffectToPlay)
        {
            currentlyPlayingSoundEffects.Add(new PlayableAudio(soundEffectToPlay));
        }

        private void PlayAudioTrack(AudioTrack audioTrackToPlay)
        {
            // TODO Stop CurrentlyPlayingAudioTrack
            CurrentlyPlayingAudioTrack = new PlayableAudio(audioTrackToPlay);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
