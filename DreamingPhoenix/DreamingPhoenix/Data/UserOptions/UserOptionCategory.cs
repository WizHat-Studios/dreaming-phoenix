using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public enum UserOptionCategory
    {
        [Description("Audio Settings")]
        Audio,
        [Description("Scene Settings")]
        Scene,
        [Description("Output Device")]
        OutputDevice,
        [Description("UI")]
        UI,
        [Description("Others")]
        Others
    }
}
