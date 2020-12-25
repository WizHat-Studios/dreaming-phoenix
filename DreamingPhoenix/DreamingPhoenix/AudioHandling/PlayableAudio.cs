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
        public event EventHandler AudioStopped;
        public Action<double, double> AudioTrackTick;

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

        private AudioFileReader audioReader;

        public AudioFileReader AudioReader
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

        private double currSeconds;

        public double CurrSeconds
        {
            get { return currSeconds; }
            set
            {
                currSeconds = value;
                //NotifyPropertyChanged();
            }
        }

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
            this.AudioOptions = audioOptions;
            //timerThread = new Thread(new ThreadStart(TimerRun)); // Tracking the Audio-Data on `Run` Method
        }

        /// <summary>
        /// Start playing the audio
        /// </summary>
        public void Play()
        {
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);
            outputDevice = new WaveOutEvent();
            AudioReader = new AudioFileReader(AudioOptions.AudioFile);
            Volume = AudioOptions.Volume;

            outputDevice.PlaybackStopped += OnAudioStopped;
            outputDevice.PlaybackStopped += (s, e) => AudioStopped?.Invoke(this, EventArgs.Empty);
            outputDevice.Volume = Volume;

            if (isAudioTrack)
            {
                fade = new FadeInOutSampleProvider(AudioReader, true);
                fade.BeginFadeIn(1000);
                outputDevice.Init(fade);
            }
            else
            {
                outputDevice.Init(AudioReader);
            }
            outputDevice.Play();
            if (isAudioTrack)
                RestartThread();

            Debug.WriteLine(string.Format("Playing Audio \"{0}\"", AudioOptions.Name));
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
                AudioOptions = audio;
                Play();
            }
        }

        public async Task PausePlay()
        {
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);

            if (isAudioTrack)
            {
                if (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    fade.BeginFadeOut(1000);
                    await Task.Delay(1000);
                    outputDevice.Pause();
                    timerThread.Interrupt();
                }
                else if (outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    outputDevice.Play();
                    RestartThread();
                    fade.BeginFadeIn(1000);
                }
            }
        }

        /// <summary>
        /// Stop playing the audio
        /// </summary>
        public async Task Stop()
        {
            bool isAudioTrack = AudioOptions.GetType() == typeof(AudioTrack);

            if (isAudioTrack)
            {
                fade.BeginFadeOut(((AudioTrack)AudioOptions).FadeOutSpeed);
                await Task.Delay(Convert.ToInt32(((AudioTrack)AudioOptions).FadeOutSpeed));
            }
            outputDevice.Stop();
            if (isAudioTrack)
            {
                timerThread.Interrupt();
                OnFadedOut?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ForceStop()
        {
            outputDevice.Stop();
            OnFadedOut = null;
        }

        private void OnAudioStopped(object sender, StoppedEventArgs e)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
                return;

            if (audioOptions.GetType() != typeof(AudioTrack))
                return;

            if (((AudioTrack)AudioOptions).NextAudioTrack == null)
                return;

            AudioOptions = ((AudioTrack)AudioOptions).NextAudioTrack;
            Play();
        }

        private void TimerRun()
        {
            try
            {
                while (true)
                {
                    if (this.outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        double ms = this.outputDevice.GetPosition() * 1000.0 / this.outputDevice.OutputWaveFormat.BitsPerSample / this.outputDevice.OutputWaveFormat.Channels * 8 / this.outputDevice.OutputWaveFormat.SampleRate;
                        AudioTrackTick?.Invoke(Math.Round(ms / 1000), Math.Round(audioReader.TotalTime.TotalSeconds));
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
