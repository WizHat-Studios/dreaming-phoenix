using System;
using System.Collections.Generic;
using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    public class AudioTrack : Audio
    {
        private AudioTrack nextAudioTrack;

        /// <summary>
        /// Next audiotrack to be played after the current finished playing
        /// </summary>
        public AudioTrack NextAudioTrack
        {
            get { return nextAudioTrack; }
            set { nextAudioTrack = value; NotifyPropertyChanged(); }
        }


        private float fadeOutSpeed;

        /// <summary>
        /// The fade out speed if the playing of the track is aborted and a next one should be played
        /// </summary>
        public float FadeOutSpeed
        {
            get { return fadeOutSpeed; }
            set { fadeOutSpeed = value; NotifyPropertyChanged(); }
        }

    }
}
