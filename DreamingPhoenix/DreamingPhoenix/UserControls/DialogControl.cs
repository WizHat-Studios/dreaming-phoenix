using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// The Dialog Control is a UserControl with special properties to be used with the DialogStack of the MainWindow
    /// </summary>
    public class DialogControl : UserControl
    {
        /// <summary>
        /// MainWindow which owns the current control and may displays it.
        /// </summary>
        internal MainWindow Owner { get; set; }
        internal event DialogClosedEventHandler DialogClosed;
        internal delegate void DialogClosedEventHandler(object sender, DialogClosedEventArgs e);
        public bool AllowExitOnBackgroundClick = true;

        public DialogControl()
        { }

        /// <summary>
        /// Closes current dialog
        /// </summary>
        public void Close(object dialogResult = null)
        {
            Owner.DialogStack.Pop();
            DialogClosed?.Invoke(this, new DialogClosedEventArgs(dialogResult));
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
