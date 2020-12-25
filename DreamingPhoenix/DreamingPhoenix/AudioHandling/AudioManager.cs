using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DreamingPhoenix.AudioHandling
{
    public class AudioManager : INotifyPropertyChanged
    {

        private ObservableCollection<PlayableAudio> currentlyPlayingSoundEffects = new ObservableCollection<PlayableAudio>();

        /// <summary>
        /// List of all currently playing sound effects
        /// </summary>
        public ObservableCollection<PlayableAudio> CurrentlyPlayingSoundEffects
        {
            get { return currentlyPlayingSoundEffects; }
            set
            {
                currentlyPlayingSoundEffects = value;
                NotifyPropertyChanged();
            }
        }

        private PlayableAudio currentlyPlayingAudioTrack = new PlayableAudio(Audio.Default);

        /// <summary>
        /// The audio track which is currently played by the application
        /// </summary>
        public PlayableAudio CurrentlyPlayingAudioTrack
        {
            get { return currentlyPlayingAudioTrack; }
            set
            {
                currentlyPlayingAudioTrack = value;
                NotifyPropertyChanged();
            }
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

        public async Task<bool> PausePlayAudioTrack()
        {
            if (CurrentlyPlayingAudioTrack == null || CurrentlyPlayingAudioTrack.AudioReader == null)
                return false;
            await CurrentlyPlayingAudioTrack.PausePlay();
            return true;
        }

        private void PlaySoundEffect(SoundEffect soundEffectToPlay)
        {
            PlayableAudio audio = new PlayableAudio(soundEffectToPlay);
            audio.AudioStopped += (s, e) =>
            {
                CurrentlyPlayingSoundEffects.Remove(audio);
            };
            CurrentlyPlayingSoundEffects.Add(audio);
            audio.Play();
        }

        private void PlayAudioTrack(AudioTrack audioTrackToPlay)
        {
            // TODO Stop CurrentlyPlayingAudioTrack
            CurrentlyPlayingAudioTrack.Play(audioTrackToPlay);
        }

        public async void StopAudio(PlayableAudio audio)
        {
            await audio.Stop();
        }

        public void StopAllAudio()
        {
            foreach (PlayableAudio audio in CurrentlyPlayingSoundEffects)
            {
                audio.ForceStop();
            }

            CurrentlyPlayingSoundEffects.Clear();

            CurrentlyPlayingAudioTrack.ForceStop();
            CurrentlyPlayingAudioTrack = new PlayableAudio(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
