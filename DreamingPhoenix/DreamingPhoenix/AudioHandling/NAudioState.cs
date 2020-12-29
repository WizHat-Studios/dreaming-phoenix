using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    /// <summary>
    /// Audio Track Reader State
    /// </summary>
    public enum NAudioState
    {
        /// <summary>
        /// Not set
        /// </summary>
        None,
        /// <summary>
        /// Audio stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// Audio currently playing
        /// </summary>
        Playing,
        /// <summary>
        /// Audio currently paused
        /// </summary>
        Paused
    }
}
