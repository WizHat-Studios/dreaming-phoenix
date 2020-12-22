using System;
using System.Collections.Generic;
using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    public class SoundEffect : Audio
    {
        // Currently empty because SoundEffects have no special properties

        /// <summary>
        /// Creates a new Audio
        /// </summary>
        /// <param name="audioFile">Audio File Path</param>
        /// <param name="name">Audio Name</param>
        public SoundEffect(string audioFile, string name) : base(audioFile, name)
        {
        }
    }
}
