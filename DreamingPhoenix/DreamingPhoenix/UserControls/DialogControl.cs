using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// The Dialog Control is a UserControl with special properties to be used with the DialogStack of the MainWindow
    /// </summary>
    public class DialogControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// MainWindow which owns the current control and may displays it.
        /// </summary>
        internal MainWindow Owner { get; set; }
        internal event DialogClosedEventHandler DialogClosed;
        internal delegate void DialogClosedEventHandler(object sender, DialogClosedEventArgs e);
        public bool AllowExitOnBackgroundClick = true;

        public DialogControl()
        {
        }

        /// <summary>
        /// Closes current dialog
        /// </summary>
        public void Close(object dialogResult = null)
        {
            Owner.DialogStack.Pop();
            DialogClosed?.Invoke(this, new DialogClosedEventArgs(dialogResult));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal class DialogClosedEventArgs : EventArgs
        {
            public object ReturnValue { get; set; }

            public DialogClosedEventArgs(object returnValue)
            {
                ReturnValue = returnValue;
            }
        }
    }
}
