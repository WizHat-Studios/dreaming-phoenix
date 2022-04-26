using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace WizHat.DreamingPhoenix.HotkeyHandling.KeyboardListener
{
    class WindowsHookHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public WindowsHookHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            // Unhook
            return Native.UnhookWindowsHookEx(handle);
        }
    }
}
