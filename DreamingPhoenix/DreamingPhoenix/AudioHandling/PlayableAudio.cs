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
    [DebuggerDisplay("{CurrentAudio.GetType() == typeof(AudioTrack) ? \"(AudioTrack)\" : \"(SoundEffect)\",nq} {CurrentAudio.Name,nq} - Volume: {System.Math.Round(AudioTrackReader.Volume * 100)}")]
    public class PlayableAudio : INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Occurs every second for a AudioTrack with the current seconds and the total seconds
        /// </summary>
        public Action<double, double> AudioTrackTick;
        /// <summary>
        /// Occurs when the Audio is stopped
        /// </summary>
        public event EventHandler AudioStopped;
        /// <summary>
        /// Occurs when the Audio is paused
        /// </summary>
        public event EventHandler AudioPaused;
        /// <summary>
        /// Occurs when the Audio is started
        /// </summary>
        public event EventHandler AudioStarted;
        #endregion

        private Thread timeTickerThread;

        private Audio currentAudio;
        /// <summary>
        /// The current Audio File
        /// </summary>
        public Audio CurrentAudio
        {
            get { return currentAudio; }
            set
            {
                currentAudio = value;
                NotifyPropertyChanged();
            }
        }

        private NAudioTrackReader audioTrackReader;
        /// <summary>
        /// The NAudio Track Reader
        /// </summary>
        public NAudioTrackReader AudioTrackReader
        {
            get { return audioTrackReader; }
            set
            {
                audioTrackReader = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new PlayableAudio
        /// </summary>
        /// <param name="audio"></param>
        public PlayableAudio(Audio audio)
        {
            CurrentAudio = audio;
        }

        #region Controls
        /// <summary>
        /// Start playing the current audio
        /// </summary>
        public void Play()
        {
            bool isAudioTrack = CurrentAudio.GetType() == typeof(AudioTrack);
            // if audio is still playing, mute it
            if (AudioTrackReader != null && AppModel.Instance.AudioManager.MixingProvider != null)
                AudioTrackReader.Volume = 0;

            // Create new Reader and subscribe to all events
            AudioTrackReader = new NAudioTrackReader(CurrentAudio.AudioFile);
            AudioTrackReader.Volume = CurrentAudio.Volume;
            AudioTrackReader.AudioStopped += (s, e) => OnAudioStopped(s, e);
            AudioTrackReader.AudioPaused += (s, e) => AudioPaused?.Invoke(this, EventArgs.Empty);
            AudioTrackReader.AudioStarted += (s, e) => AudioStarted?.Invoke(this, EventArgs.Empty);
            AudioStarted?.Invoke(this, EventArgs.Empty);

            CurrentAudio.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentAudio.Volume))
                    AudioTrackReader.Volume = CurrentAudio.Volume;
            };

            // Insert Reader to MixingSampleProvider
            ISampleProvider sampleProvider = AudioTrackReader;
            if (AppModel.Instance.AudioManager.MixingProvider == null)
            {
                // if MixingSampleProvider is null, instantiate with non empty list
                List<ISampleProvider> temp = new List<ISampleProvider>();
                temp.Add(sampleProvider);
                AppModel.Instance.AudioManager.MixingProvider = new MixingSampleProvider(temp);
            }
            else
            {
                AppModel.Instance.AudioManager.MixingProvider.AddMixerInput(sampleProvider);
            }

            // if AudioTrack, start ticker
            if (isAudioTrack)
                RestartThread();

            Debug.WriteLine(string.Format("Playing Audio \"{0}\"", CurrentAudio.Name));
        }

        /// <summary>
        /// Start playing a new audio and stop current audio
        /// </summary>
        /// <param name="audio">The new audio</param>
        public void Play(Audio audio)
        {
            // Clear all subscribers from event, because we instaniate a new audio
            // (Only affects PlayableAudio internally. All external classes are still subscribed to PlayableAudio)
            AudioTrackReader?.ClearAudioStoppedEvent();
            bool isAudioTrack = CurrentAudio.GetType() == typeof(AudioTrack);

            // if audio is currently playing, fade out and start playing on stopped
            if (AudioTrackReader != null && AudioTrackReader.State == NAudioState.Playing)
            {
                AudioTrackReader.AudioStopped += (s, e) =>
                {
                    if (isAudioTrack)
                        timeTickerThread.Interrupt();
                    Play(audio);
                };
                Stop();
            }
            else
            {
                // Nothing is playing, just replace it and play
                CurrentAudio = audio;
                Play();
            }
        }

        /// <summary>
        /// Pause or Resume the current AudioTrack (Only for <seealso cref="AudioTrack"/>)
        /// </summary>
        /// <returns></returns>
        public async Task PausePlay()
        {
            bool isAudioTrack = CurrentAudio.GetType() == typeof(AudioTrack);

            // Only a Audio Track is pausable
            if (!isAudioTrack)
                return;

            // if it's currently playing, fade it out and stop the ticker
            if (AudioTrackReader.State == NAudioState.Playing)
            {
                AudioTrackReader.Pause(500);
                await Task.Delay(500);
                timeTickerThread.Interrupt();
            }
            // if it's currently paused, fade it in and start the ticker
            else if (AudioTrackReader.State == NAudioState.Paused)
            {
                AudioTrackReader.Play(500);
                RestartThread();
            }
        }

        /// <summary>
        /// Stop playing the audio
        /// </summary>
        /// <param name="force">if true, fadeoutspeed 0 is used, otherwise the audio fadeoutspeed is used</param>
        public void Stop(bool force = false)
        {
            //AudioReader.AudioStopped += (s, e) => OnAudioStopped(s, e);
            bool isAudioTrack = CurrentAudio.GetType() == typeof(AudioTrack);
            double fadeOutSpeed = 0;

            // Use the FadeOutSpeed of the AudioTrack if it's not a force stop
            if (isAudioTrack && !force)
                fadeOutSpeed = ((AudioTrack)CurrentAudio).FadeOutSpeed;

            // On force stop, reset Audio to default
            if (force)
            {
                if (isAudioTrack)
                    CurrentAudio = AudioTrack.Default;
                else
                    CurrentAudio = SoundEffect.Default;
            }

            // Now stop Reader
            if (AudioTrackReader != null)
                AudioTrackReader.Stop(fadeOutSpeed);
        }
        #endregion

        /// <summary>
        /// Gets called if Reader invokes AudioStopped
        /// </summary>
        private void OnAudioStopped(object sender, EventArgs e)
        {
            bool isAudioTrack = currentAudio.GetType() == typeof(AudioTrack);
            if (isAudioTrack && timeTickerThread != null)
                timeTickerThread.Interrupt();

            // Prioritize repeat
            if (CurrentAudio.Repeat)
            {
                Play(CurrentAudio);
                return;
            }
            AudioStopped?.Invoke(this, EventArgs.Empty);

            // if sound track, do nothing else
            if (!isAudioTrack)
                return;

            // Play next track
            if (((AudioTrack)CurrentAudio).NextAudioTrack != null)
            {
                Play(((AudioTrack)CurrentAudio).NextAudioTrack);
                return;
            }

            // Reset to default AudioTrack
            if (((AudioTrack)CurrentAudio).NextAudioTrack == null || !((AudioTrack)CurrentAudio).Repeat)
            {
                CurrentAudio = AudioTrack.Default;
                return;
            }

        }

        #region Audio Ticker
        /// <summary>
        /// Tick every second and call event with current and total seconds of reader
        /// </summary>
        private async void TimerRun()
        {
            try
            {
                while (true)
                {
                    // Only tick if audio is playing
                    if (AudioTrackReader.State == NAudioState.Playing)
                        AudioTrackTick?.Invoke(AudioTrackReader.CurrentTime.TotalSeconds, Math.Round(AudioTrackReader.TotalTime.TotalSeconds));
                    else
                        break;

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Interrupt and start thread
        /// </summary>
        private void RestartThread()
        {
            if (timeTickerThread != null)
                timeTickerThread.Interrupt();
            timeTickerThread = new Thread(new ThreadStart(TimerRun));
            timeTickerThread.Start();
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
