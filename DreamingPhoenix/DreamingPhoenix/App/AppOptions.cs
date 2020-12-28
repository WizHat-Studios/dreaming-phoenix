using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DreamingPhoenix
{
    public class AppOptions : INotifyPropertyChanged
    {

        private bool extendedModeEnabled = false;

        public bool ExtendedModeEnabled
        {
            get { return extendedModeEnabled; }
            set { extendedModeEnabled = value; NotifyPropertyChanged(); }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
