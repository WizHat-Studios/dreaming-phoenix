using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace DreamingPhoenix.AudioHandling
{
    public class PlayableAudio : INotifyPropertyChanged
    {
        private Audio audioOptions;
        private AudioFileReader audioFileReader;
        private WaveOutEvent outputDevice;
        private FadeInOutSampleProvider fade;
        private event EventHandler OnFadedOut;
        private float volume;

        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                NotifyPropertyChanged();
            }
        }

        public PlayableAudio(Audio audioOptions)
        {
            this.audioOptions = audioOptions;
        }

        /// <summary>
        /// Start playing the audio
        /// </summary>
        public void Play()
        {
            outputDevice = new WaveOutEvent();
            audioFileReader = new AudioFileReader(audioOptions.AudioFile);
            Volume = audioOptions.Volume;

            outputDevice.PlaybackStopped += OnAudioStopped;
            outputDevice.Volume = Volume;
            if (audioOptions.GetType() == typeof(AudioTrack))
            {
                fade = new FadeInOutSampleProvider(audioFileReader, false);
                outputDevice.Init(fade);
            }
            else
            {
                outputDevice.Init(audioFileReader);
            }
            outputDevice.Play();
        }

        public void Play(Audio audio)
        {
            // Clear all subscribers from event
            OnFadedOut = null;

            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                OnFadedOut += (s, e) => Play(audio);
                Stop();
            }
            else
            {
                audioOptions = audio;
                Play();
            }
        }

        /// <summary>
        /// Stop playing the audio
        /// </summary>
        public async Task Stop()
        {
            if (audioOptions.GetType() != typeof(AudioTrack))
                return;

            fade.BeginFadeOut(((AudioTrack)audioOptions).FadeOutSpeed);
            await Task.Delay(Convert.ToInt32(((AudioTrack)audioOptions).FadeOutSpeed));
            outputDevice.Stop();
            OnFadedOut?.Invoke(this, EventArgs.Empty);
        }

        public void ForceStop()
        {
            outputDevice.Stop();
            OnFadedOut = null;
        }

        private void OnAudioStopped(object sender, StoppedEventArgs e)
        {
            if (audioOptions.GetType() != typeof(AudioTrack))
                return;

            if (((AudioTrack)audioOptions).NextAudioTrack == null)
                return;

            audioOptions = ((AudioTrack)audioOptions).NextAudioTrack;
            Play();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
