using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace DreamingPhoenix.AudioHandling
{
    public class PlayableAudio : INotifyPropertyChanged
    {
        public Action<double, double> AudioTrackTick;
        public event EventHandler AudioStopped;

        private Audio audioOptions;

        public Audio AudioOptions
        {
            get { return audioOptions; }
            set
            {
                audioOptions = value;
                NotifyPropertyChanged();
            }
        }

        private NAudioTrackReader audioReader;

        public NAudioTrackReader AudioReader
        {
            get
            {
                return audioReader;
            }
            set
            {
                audioReader = value;
                NotifyPropertyChanged();
            }
        }

        private Thread timerThread;

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
            AudioOptions = audioOptions;
        }

        /// <summary>
        /// Start playing the audio
        /// </summary>
        public void Play()
        {
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);
            if (AudioReader != null && AppModel.Instance.AudioManager.MixingProvider != null)
                AudioReader.Volume = 0;

            AudioReader = new NAudioTrackReader(AudioOptions.AudioFile);
            AudioReader.Volume = AudioOptions.Volume;
            Volume = AudioOptions.Volume;
            AudioReader.AudioStopped += OnAudioStopped;

            ISampleProvider sampleProvider = AudioReader;

            if (AppModel.Instance.AudioManager.MixingProvider == null)
            {
                List<ISampleProvider> temp = new List<ISampleProvider>();
                temp.Add(sampleProvider);
                AppModel.Instance.AudioManager.MixingProvider = new MixingSampleProvider(temp);
            }
            else
            {
                AppModel.Instance.AudioManager.MixingProvider.AddMixerInput(sampleProvider);
            }

            if (isAudioTrack)
                RestartThread();

            Debug.WriteLine(string.Format("Playing Audio \"{0}\"", AudioOptions.Name));
        }

        public void Play(Audio audio)
        {
            // Clear all subscribers from event
            AudioReader?.ClearAudioStoppedEvent();
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);

            // If audio is currently playing, fade out and start new on faded out
            if (AudioReader != null && AudioReader.State == NAudioState.Playing)
            {
                AudioReader.AudioStopped += (s, e) =>
                {
                    if (isAudioTrack)
                        timerThread.Interrupt();
                    Play(audio);
                };
                Stop();
            }
            else
            {
                AudioOptions = audio;
                Play();
            }
        }

        public async Task PausePlay()
        {
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);

            // Only a Audio Track is pausable
            if (!isAudioTrack)
                return;

            if (AudioReader.State == NAudioState.Playing)
            {
                AudioReader.Pause(500);
                await Task.Delay(500);
                timerThread.Interrupt();
            }
            else if (AudioReader.State == NAudioState.Paused)
            {
                AudioReader.Play(500);
                RestartThread();
            }
        }

        /// <summary>
        /// Stop playing the audio
        /// </summary>
        public void Stop()
        {
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);
            double fadeOutSpeed = 0;

            if (isAudioTrack)
                fadeOutSpeed = ((AudioTrack)AudioOptions).FadeOutSpeed;

            AudioReader.Stop(fadeOutSpeed);
        }

        private void OnAudioStopped(object sender, EventArgs e)
        {
            AudioStopped?.Invoke(this, EventArgs.Empty);
            if (audioOptions.GetType() != typeof(AudioTrack))
                return;

            if (((AudioTrack)AudioOptions).NextAudioTrack == null)
            {
                //AudioTrackTick?.Invoke(-1, -1);
                AudioOptions = Audio.Default;
                return;
            }

            Play(((AudioTrack)AudioOptions).NextAudioTrack);
        }

        private void TimerRun()
        {
            try
            {
                while (true)
                {
                    //if (this.outputDevice.PlaybackState == PlaybackState.Playing)
                    if (this.AudioReader.State == NAudioState.Playing)
                    {
                        //double ms = this.outputDevice.GetPosition() * 1000.0 / this.outputDevice.OutputWaveFormat.BitsPerSample / this.outputDevice.OutputWaveFormat.Channels * 8 / this.outputDevice.OutputWaveFormat.SampleRate;
                        AudioTrackTick?.Invoke(AudioReader.CurrentTime.TotalSeconds, Math.Round(AudioReader.TotalTime.TotalSeconds));
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ThreadInterruptedException) { }
        }

        private void RestartThread()
        {
            timerThread = new Thread(new ThreadStart(TimerRun));
            timerThread.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
