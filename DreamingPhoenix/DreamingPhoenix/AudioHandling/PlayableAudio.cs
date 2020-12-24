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
                NotifyPropertyChanged();
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
            timerThread = new Thread(new ThreadStart(TimerRun)); // Tracking the Audio-Data on `Run` Method
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
            outputDevice.Volume = Volume;

            if (isAudioTrack)
            {
                fade = new FadeInOutSampleProvider(AudioReader, false);
                outputDevice.Init(fade);
            }
            else
            {
                outputDevice.Init(AudioReader);
            }
            outputDevice.Play();
            //if (isAudioTrack)
            //    timerThread.Start();

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

        /// <summary>
        /// Stop playing the audio
        /// </summary>
        public async Task Stop()
        {
            if (AudioOptions.GetType() != typeof(AudioTrack))
                return;

            fade.BeginFadeOut(((AudioTrack)AudioOptions).FadeOutSpeed);
            await Task.Delay(Convert.ToInt32(((AudioTrack)AudioOptions).FadeOutSpeed));
            outputDevice.Stop();
            //timerThread.Interrupt();
            OnFadedOut?.Invoke(this, EventArgs.Empty);
        }

        public void ForceStop()
        {
            outputDevice.Stop();
            OnFadedOut = null;
        }

        private void OnAudioStopped(object sender, StoppedEventArgs e)
        {
            if (AudioOptions.GetType() != typeof(AudioTrack))
                return;

            if (((AudioTrack)AudioOptions).NextAudioTrack == null)
                return;

            AudioOptions = ((AudioTrack)AudioOptions).NextAudioTrack;
            Play();
        }

        private void TimerRun()
        {
            while (true)
            { // Create an infinite loop
                if (this.outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    double ms = this.outputDevice.GetPosition() * 1000.0 / this.outputDevice.OutputWaveFormat.BitsPerSample / this.outputDevice.OutputWaveFormat.Channels * 8 / this.outputDevice.OutputWaveFormat.SampleRate;
                    //Debug.WriteLine("Milliseconds Played: " + ms);
                    CurrSeconds = ms / 1000;
                }

                Thread.Sleep(1000); // Sleep for 1 second
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
