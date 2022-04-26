using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WizHat.DreamingPhoenix.HotkeyHandling.KeyboardListener
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardEventData
    {
        public int VirtualKeyCode;
        public int HardwareScanCode;
        public int Flags;
        public int Time;
        public IntPtr ExtraInfo;
    }
}
