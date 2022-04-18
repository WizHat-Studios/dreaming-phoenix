﻿using WizHat.DreamingPhoenix.AudioHandling;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.ExternalAudio;
using WizHat.DreamingPhoenix.Extensions;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for FileDragDrop.xaml
    /// </summary>
    public partial class YouTubeDownloader : UserControl, INotifyPropertyChanged
    {
        public event EventHandler OperationProcessed;

        private string downloadedPath;
        public string DownloadedPath
        {
            get { return downloadedPath; }
            set
            {
                downloadedPath = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DownloadedFileName));
            }
        }

        public string DownloadedFileName
        {
            get
            {
                if (DownloadedPath == "")
                    return "";

                return Path.GetFileNameWithoutExtension(downloadedPath);
            }
        }

        private string youtubeURL;
        public string YouTubeURL
        {
            get { return youtubeURL; }
            set
            {
                youtubeURL = value;
                NotifyPropertyChanged();
            }
        }

        public YouTubeDownloader()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            YouTubeAudio youtubeAudio = new();
            youtubeAudio.OnDownloadProgress += (sender, e) =>
            {
                pgb_downloadProgress.SetPercent(e.Value);
            };
            youtubeAudio.OnGatheredInformations += (sender, e) =>
            {
                DownloadedPath = e.Value;
            };

            if (await youtubeAudio.DownloadAudio(YouTubeURL))
                btn_add.IsEnabled = true;
        }

        private void AudioTrack_Clicked(object sender, RoutedEventArgs e)
        {
            if (tglbtn_soundEffect == null || tglbtn_audioTrack == null)
                return;

            tglbtn_soundEffect.IsChecked = !tglbtn_audioTrack.IsChecked;
        }

        private void SoundEffect_Clicked(object sender, RoutedEventArgs e)
        {
            if (tglbtn_soundEffect == null || tglbtn_audioTrack == null)
                return;

            tglbtn_audioTrack.IsChecked = !tglbtn_soundEffect.IsChecked;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (tglbtn_audioTrack.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new AudioTrack(downloadedPath, Path.GetFileNameWithoutExtension(downloadedPath)));
            }
            else if (tglbtn_soundEffect.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new SoundEffect(downloadedPath, Path.GetFileNameWithoutExtension(downloadedPath)));
            }

            Close();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            OperationProcessed?.Invoke(this, EventArgs.Empty);
            YouTubeURL = "";
            DownloadedPath = "";
            pgb_downloadProgress.SetPercent(0);
            btn_add.IsEnabled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}