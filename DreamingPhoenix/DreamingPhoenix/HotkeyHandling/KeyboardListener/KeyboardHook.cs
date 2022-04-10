using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WizHat.DreamingPhoenix.HotkeyHandling.KeyboardListener
{
    public class KeyboardHook : IDisposable
    {
        public delegate void OnKeyboardEventHandler(object sender, KeyboardEventArgs e);

        public event OnKeyboardEventHandler OnKeyboard;

        private WindowsHookHandle _hookHandle;
        private Native.LowLevelKeyboardProc _keyboardProc;

        public KeyboardHook()
        {
            // Assign keyboard message handler
            _keyboardProc = KeyboardCallback;
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule module = currentProcess.MainModule)
                {
                    // Set windows hook
                    _hookHandle = new WindowsHookHandle(Native.SetWindowsHookEx((int)WindowsHookType.WH_KEYBOARD_LL, _keyboardProc,
                        Native.GetModuleHandle(module.ModuleName), 0));
                }
            }
        }

        private IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Message should be ignored if nCode is negative
            if (nCode >= 0)
            {
                // Get key state
                KeyState state = (KeyState)wParam.ToInt32();
                // Get key data
                KeyboardEventData data = Marshal.PtrToStructure<KeyboardEventData>(lParam);
                // Create event arguments
                KeyboardEventArgs args = new KeyboardEventArgs(state, data);
                // Invoke event
                OnKeyboard?.Invoke(this, args);
                // Return nonzero value to prevent passing on
                if (args.Handled)
                {
                    return (IntPtr)1;
                }
            }

            return Native.CallNextHookEx(_hookHandle.DangerousGetHandle(), nCode, wParam, lParam);
        }

        public void Dispose()
        {
            _hookHandle?.Dispose();
        }
    }
}
