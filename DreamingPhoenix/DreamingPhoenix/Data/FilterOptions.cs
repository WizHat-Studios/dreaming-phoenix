using WizHat.DreamingPhoenix.AudioHandling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using WizHat.DreamingPhoenix.Sorting;
using WizHat.DreamingPhoenix.AudioProperties;

namespace WizHat.DreamingPhoenix.Data
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

        private SortType sortType;

        public SortType SortType
        {
            get { return sortType; }
            set { sortType = value; NotifyPropertyChanged(); }
        }

        private bool includeSoundEffects = true;

        public bool IncludeSoundEffects
        {
            get { return includeSoundEffects; }
            set { includeSoundEffects = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<SelectableTag> selectedTags = new ObservableCollection<SelectableTag>();

        public ObservableCollection<SelectableTag> SelectedTags
        {
            get { return selectedTags; }
            set { selectedTags = value; NotifyPropertyChanged(); }
        }

        public void UpdateTags()
        {
            AppModel.Instance.UpdateAvailableTags();
            ObservableCollection<SelectableTag> updatedSelectedTags = new ObservableCollection<SelectableTag>();

            foreach (Tag tag in AppModel.Instance.AvailableTags)
            {
                bool isSelected = false;
                foreach (SelectableTag selectedTag in selectedTags)
                {
                    if (tag.Text == selectedTag.Text)
                    {
                        isSelected = selectedTag.Selected;
                        break;
                    }
                }

                updatedSelectedTags.Add(new SelectableTag() { Text = tag.Text, Selected = isSelected });
            }

            SelectedTags = new ObservableCollection<SelectableTag>(updatedSelectedTags.OrderBy(x => x.Text).ToList());
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
