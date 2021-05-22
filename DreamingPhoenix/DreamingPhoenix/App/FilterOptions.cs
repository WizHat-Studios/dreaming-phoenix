using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace DreamingPhoenix
{
    public class FilterOptions : INotifyPropertyChanged, ICloneable
    {
        private string searchTerm = "";

        /// <summary>
        /// String used by the searchResultAudioList to filter the audio results
        /// </summary>
        public string SearchTerm
        {
            get { return searchTerm; }
            set { searchTerm = value; NotifyPropertyChanged(); Debug.WriteLine("new value is " + value); }
        }

        private SortDirection sortDirection;

        public SortDirection SortDirection
        {
            get { return sortDirection; }
            set { sortDirection = value; NotifyPropertyChanged(); }
        }


        private bool includeAudioTracks = true;

        public bool IncludeAudioTracks
        {
            get { return includeAudioTracks; }
            set { includeAudioTracks = value; NotifyPropertyChanged(); }
        }

        private bool includeSoundEffects = true;

        public bool IncludeSoundEffects
        {
            get { return includeSoundEffects; }
            set { includeSoundEffects = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
