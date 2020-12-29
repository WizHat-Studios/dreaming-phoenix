using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace DreamingPhoenix.AudioHandling
{
    public class AudioManager : INotifyPropertyChanged
    {
        public WaveOutEvent OutputDevice = new WaveOutEvent();
        public MixingSampleProvider MixingProvider;

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

        public AudioManager()
        {
            BindingOperations.EnableCollectionSynchronization(CurrentlyPlayingSoundEffects, false);
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
            PlayableAudio audio = new PlayableAudio(soundEffectToPlay);
            audio.AudioStopped += (s, e) => CurrentlyPlayingSoundEffects.Remove(audio);
            CurrentlyPlayingSoundEffects.Add(audio);
            audio.Play();

            InitializeMixer();
        }

        private void PlayAudioTrack(AudioTrack audioTrackToPlay)
        {
            CurrentlyPlayingAudioTrack.Play(audioTrackToPlay);
            InitializeMixer();
        }

        private void InitializeMixer()
        {
            if (OutputDevice != null && OutputDevice.PlaybackState == PlaybackState.Stopped)
            {
                OutputDevice.Init(MixingProvider);
                OutputDevice.Play();
            }
        }

        public async Task<bool> PausePlayAudioTrack()
        {
            if (CurrentlyPlayingAudioTrack == null || CurrentlyPlayingAudioTrack.AudioReader == null)
                return false;
            await CurrentlyPlayingAudioTrack.PausePlay();
            return true;
        }

        public void StopAudio(PlayableAudio audio)
        {
            audio.Stop();
        }

        public void StopAllAudio()
        {
            CurrentlyPlayingAudioTrack.Stop();
            foreach (PlayableAudio audio in CurrentlyPlayingSoundEffects)
            {
                audio.Stop();
            }

            //MixingProvider.RemoveAllMixerInputs();
            CurrentlyPlayingSoundEffects.Clear();
            CurrentlyPlayingAudioTrack = new PlayableAudio(Audio.Default);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
