using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DreamingPhoenix.HotkeyHandling.KeyboardListener
{
    public class KeyboardEventArgs : HandledEventArgs
    {
        public KeyState KeyState { get; }
        public KeyboardEventData KeyboardData { get; }
        public char Character => Convert.ToChar(Native.MapVirtualKey((uint)KeyboardData.VirtualKeyCode, 2));
        public Key Key => KeyInterop.KeyFromVirtualKey(KeyboardData.VirtualKeyCode);

        public KeyboardEventArgs(KeyState keyState, KeyboardEventData keyboardData)
        {
            KeyState = keyState;
            KeyboardData = keyboardData;
        }
    }
}
