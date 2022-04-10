﻿using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
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
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.Styles.Scheme;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for UserApplicationOptions.xaml
    /// </summary>
    public partial class UserApplicationOptions : UserControl
    {
        public AppOptions Options { get; set; } = AppModel.Instance.Options;

        public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public UserApplicationOptions()
        {
            InitializeComponent();
            DataContext = this;

            List<string> outputs = new List<string>();
            var enumerator = new MMDeviceEnumerator();
            foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                outputs.Add(endpoint.FriendlyName);

            foreach (ColorScheme theme in ColorScheme.Themes)
            {
                cbox_themes.Items.Add(theme.ThemeDisplayName);
            }


            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                string productName = WaveOut.GetCapabilities(n).ProductName;
                if (outputs.Exists(o => o.ToLower().StartsWith(productName.ToLower())))
                    productName = outputs.First(o => o.ToLower().StartsWith(productName.ToLower()));
                cbox_outputDevices.Items.Add(productName);
            }
        }

        private void cbox_outputDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppModel.Instance.ChangeOutputDevice(AppModel.Instance.Options.DefaultOutputDevice - 1);
        }

        private void btn_readAllFilesFromDisk_Click(object sender, RoutedEventArgs e)
        {
            foreach (Audio audio in AppModel.Instance.AudioList)
            {
                audio.CheckIfFileExistsOnDisk();
            }
        }

        private void btn_cleanCache_Click(object sender, RoutedEventArgs e)
        {
            Cache.CacheManager.Instance.CleanUpCache();
        }
    }
}
