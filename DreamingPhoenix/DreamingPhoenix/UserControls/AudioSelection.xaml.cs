using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for AudioSelection.xaml
    /// </summary>
    public partial class AudioSelection : DialogControl, INotifyPropertyChanged
    {
        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        private Audio selectedAudio;

        public Audio SelectedAudio
        {
            get { return selectedAudio; }
            set { selectedAudio = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Audio> audioList;

        public ObservableCollection<Audio> AudioList
        {
            get { return audioList; }
            set { audioList = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<Audio> filteredAudioList;

        public ObservableCollection<Audio> FilteredAudioList
        {
            get { return filteredAudioList; }
            set { filteredAudioList = value; NotifyPropertyChanged(); }
        }

        private string searchText = "";

        public string SearchText
        {
            get { return searchText; }
            set 
            { 
                searchText = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SearchActive));
                FilteredAudioList = new ObservableCollection<Audio>(AudioList.Where(x => x.Name.ToLower().Contains(SearchText.ToLower())));
            }
        }

        public bool SearchActive { get { return !string.IsNullOrEmpty(SearchText); } }

        public AudioSelection(Type selectionType, Audio previousAudio = null)
        {
            InitializeComponent();
            this.DataContext = this;
            SelectedAudio = previousAudio;
            AudioList = new ObservableCollection<Audio>(AppModelInstance.AudioList.Where(x => x.GetType() == selectionType));
            SearchText = "";
            ((INotifyCollectionChanged)lbox_selectableAudioList.Items).CollectionChanged += DetermineAudioListPrompt;
        }

        private void DetermineAudioListPrompt(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (FilteredAudioList.Count == 0)
            {
                if (AudioList.Count == 0)
                {
                    grid_emptyListboxPrompt.Visibility = Visibility.Visible;
                    grid_noSearchListboxPrompt.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_emptyListboxPrompt.Visibility = Visibility.Collapsed;
                    grid_noSearchListboxPrompt.Visibility = Visibility.Visible;
                }
            }
            else
            {
                grid_emptyListboxPrompt.Visibility = Visibility.Collapsed;
                grid_noSearchListboxPrompt.Visibility = Visibility.Collapsed;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close(SelectedAudio);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AudioListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close(SelectedAudio);
        }
    }
}
