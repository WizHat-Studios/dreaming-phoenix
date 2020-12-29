using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    /// <summary>
    /// Short audio
    /// </summary>
    [DebuggerDisplay("(SoundEffect) {Name,np} - Volume: {System.Math.Round(Volume * 100),np}")]
    public class SoundEffect : Audio
    {
        /// <summary>
        /// Creates a new empty SoundEffect
        /// </summary>
        public SoundEffect()
        {
        }

        /// <summary>
        /// Creates a new SoundEffect
        /// </summary>
        /// <param name="audioFile">Audio File Path</param>
        /// <param name="name">Audio Name</param>
        public SoundEffect(string audioFile, string name) : base(audioFile, name)
        {
            if (Volume == 0)
                Volume = AppModel.Instance.Options.DefaultSoundEffectVolume;
        }

        /// <summary>
        /// Get a default SoundEffect
        /// </summary>

        public static readonly SoundEffect Default = new SoundEffect()
        {
            Name = "",
            AudioFile = ""
        };
    }
}
