using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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
using WizHat.DreamingPhoenix.Converter;
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.Data.UserOptions;
using WizHat.DreamingPhoenix.Styles.Scheme;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for UserApplicationOptions.xaml
    /// </summary>
    public partial class UserApplicationOptions : DialogControl
    {
        public RelayCommand ReadAllFilesFromDiskCommand { get; set; }
        public RelayCommand CleanCacheCommand { get; set; }

        public AppOptions Options { get; set; } = AppModel.Instance.Options;

        public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public ObservableCollection<object> Outputs { get; set; } = new();
        public ObservableCollection<object> Themes { get; set; } = new();

        public UserApplicationOptions()
        {
            InitializeComponent();
            DataContext = this;

            #region ListBox Grouping
            lsb_options.Items.IsLiveGrouping = true;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lsb_options.ItemsSource);
            PropertyGroupDescription groupDescription = new("Category", new UserOptionCategoryConverter());
            view.GroupDescriptions.Add(groupDescription);
            #endregion

            #region Commands
            ReadAllFilesFromDiskCommand = new((o) => ReadAllFilesFromDisk());
            CleanCacheCommand = new((o) => CleanCache());
            #endregion

            foreach (ColorScheme theme in ColorScheme.Themes)
            {
                Themes.Add(theme.ThemeDisplayName);
            }

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            List<string> tempOutputs = new();
            Outputs.Add(WaveOut.GetCapabilities(-1).ProductName);
            foreach (var endpoint in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                tempOutputs.Add(endpoint.FriendlyName);

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                string productName = WaveOut.GetCapabilities(n).ProductName;
                if (tempOutputs.Exists(o => o.ToLower().StartsWith(productName.ToLower())))
                    productName = tempOutputs.First(o => o.ToLower().StartsWith(productName.ToLower()));
                Outputs.Add(productName);
            }
        }

        private void ReadAllFilesFromDisk()
        {
            foreach (Audio audio in AppModel.Instance.AudioList)
            {
                audio.CheckIfFileExistsOnDisk();
            }
        }

        private void CleanCache()
        {
            Cache.CacheManager.Instance.CleanUpCache();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.SaveData();
            Close();
        }
    }
}
