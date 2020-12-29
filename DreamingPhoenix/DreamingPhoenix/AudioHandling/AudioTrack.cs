using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    /// <summary>
    /// Longer audio with repeat and fade out
    /// </summary>
    [DebuggerDisplay("(AudioTrack) {Name,np} - Volume: {System.Math.Round(Volume * 100),np}")]
    public class AudioTrack : Audio
    {
        private AudioTrack nextAudioTrack;

        /// <summary>
        /// Next audiotrack to be played after the current finished playing
        /// </summary>
        public AudioTrack NextAudioTrack
        {
            get { return nextAudioTrack; }
            set
            {
                nextAudioTrack = value;
                NotifyPropertyChanged();
            }
        }

        private double fadeOutSpeed;

        /// <summary>
        /// The fade out speed if the playing of the track is aborted and a next one should be played
        /// </summary>
        public double FadeOutSpeed
        {
            get { return fadeOutSpeed; }
            set
            {
                fadeOutSpeed = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new empty AudioTrack
        /// </summary>
        public AudioTrack()
        {
        }

        /// <summary>
        /// Creates a new AudioTrack
        /// </summary>
        /// <param name="audioFile">Audio File Path</param>
        /// <param name="name">Audio Name</param>
        public AudioTrack(string audioFile, string name) : base(audioFile, name)
        {
            if (Volume == 0)
                Volume = AppModel.Instance.Options.DefaultAudioTrackVolume;
        }

        /// <summary>
        /// Get a default AudioTrack
        /// </summary>
        public static readonly AudioTrack Default = new AudioTrack()
        {
            Name = "",
            AudioFile = ""
        };

        public override string ToString()
        {
            return Name;
        }
    }
}
