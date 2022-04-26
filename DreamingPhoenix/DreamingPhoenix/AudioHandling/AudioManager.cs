using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.AudioHandling
{
    /// <summary>
    /// Manager for all kind of audio.
    /// Play, Pause and Stop audios
    /// </summary>
    [DebuggerDisplay("Playing Sounds: {CurrentlyPlayingSoundEffects.Count} - Audio Track: {CurrentlyPlayingAudioTrack.CurrentAudio.Name,nq}")]
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

        private PlayableAudio currentlyPlayingAudioTrack = new PlayableAudio(AudioTrack.Default);
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

        /// <summary>
        /// Play new audio
        /// </summary>
        /// <param name="audioToPlay">The new audio to play</param>
        public void PlayAudio(Audio audioToPlay)
        {
            if (!File.Exists(audioToPlay.AudioFile))
            {
                audioToPlay.IsAudioFilePathValid = false;
                MessageBox.Show(string.Format("Failed to play the audio '{0}' because the file at '{1}' could not be found! Please check if the file exist or reimport the file in the properties.", audioToPlay.Name, audioToPlay.AudioFile), "Failed to play audio", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


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

        public void PlayScene(Scene sceneToPlay)
        {
            // Stop all sound effects
            foreach (PlayableAudio sf in CurrentlyPlayingSoundEffects)
            {
                StopAudio(sf, false, true);
            }

            CurrentlyPlayingSoundEffects.Clear();

            // If no audio track should be playing stop the current audio track else transition to the audio track.
            if (sceneToPlay.SceneAudioTrack != null)
                PlayAudio(sceneToPlay.SceneAudioTrack);
            else
                StopAudio(CurrentlyPlayingAudioTrack);

            foreach (SoundEffect sf in sceneToPlay.SceneSoundEffects)
            {
                PlayAudio(sf);
            }
        }

        #region Controls
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
                OutputDevice.Stop();
                OutputDevice = new WaveOutEvent() { DeviceNumber = AppModel.Instance.Options.DefaultOutputDevice };
                OutputDevice.Init(MixingProvider);
                OutputDevice.Play();
            }
        }

        /// <summary>
        /// Pause or Resume the current AudioTrack (Only for <seealso cref="AudioTrack"/>)
        /// </summary>
        /// <returns>if true, audio is paused or resumed, if false, audio couldn't be paused or resumed</returns>
        public async Task<bool> PausePlayAudio()
        {
            if (CurrentlyPlayingAudioTrack == null || CurrentlyPlayingAudioTrack.AudioTrackReader == null)
                return false;
            await CurrentlyPlayingAudioTrack.PausePlay();
            return true;
        }

        public bool PlayNextTrack()
        {
            if (CurrentlyPlayingAudioTrack == null || CurrentlyPlayingAudioTrack.AudioTrackReader == null)
                return false;
            if (((AudioTrack)CurrentlyPlayingAudioTrack.CurrentAudio).NextAudioTrack == null || !((AudioTrack)CurrentlyPlayingAudioTrack.CurrentAudio).NextAudioTrack.IsAudioFilePathValid)
                return false;
            CurrentlyPlayingAudioTrack.PlayNextTrack();
            return true;
        }

        /// <summary>
        /// Stop specific audio with FadeOutSpeed
        /// </summary>
        /// <param name="audio">The audio to stop</param>
        public void StopAudio(PlayableAudio audio, bool hardStop = false, bool softStop = false)
        {
            audio.Stop(hardStop, softStop);
        }

        /// <summary>
        /// Force stop all currently playing audios
        /// </summary>
        public void StopAllAudio()
        {
            CurrentlyPlayingAudioTrack.Stop(true);
            foreach (PlayableAudio audio in CurrentlyPlayingSoundEffects)
            {
                audio.Stop(true);
            }

            CurrentlyPlayingSoundEffects.Clear();
            CurrentlyPlayingAudioTrack = new PlayableAudio(AudioTrack.Default);
        }
        #endregion

        public bool IsWrongSampleRate(string fileName)
        {
            return GetSampleRate(fileName) != 44100;
        }

        private int GetSampleRate(string fileName)
        {
            WaveStream readerStream = null;
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new MediaFoundationReader(fileName);
            }
            else if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                readerStream = new MediaFoundationReader(fileName);
            }

            int sampleRate = readerStream.WaveFormat.SampleRate;
            readerStream.Close();
            return sampleRate;
        }

        public bool ConvertToSampleRate(ref string fileName)
        {
            WaveStream readerStream = null;
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new Mp3FileReader(fileName);
            }
            else if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                readerStream = new MediaFoundationReader(fileName);
            }

            string outFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_Converted.wav");
            if (File.Exists(outFile))
            {
                MessageBox.Show(string.Format("File {0} already exists", outFile));
                return false;
            }

            int outRate = 44100;
            if (readerStream.WaveFormat.SampleRate == outRate)
            {
                return false;
            }

            var outFormat = new WaveFormat(outRate, readerStream.WaveFormat.Channels);
            var resampler = new MediaFoundationResampler(readerStream, outFormat);
            try
            {
                WaveFileWriter.CreateWaveFile(outFile, resampler);
            }
            catch (Exception)
            {
                return false;
            }

            resampler.Dispose();
            readerStream.Close();
            File.Delete(fileName);
            File.Move(outFile, Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "." + Path.GetExtension(outFile)));

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
