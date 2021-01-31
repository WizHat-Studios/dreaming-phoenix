using DreamingPhoenix.AudioHandling;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for FileDragDrop.xaml
    /// </summary>
    public partial class FileDragDrop : UserControl
    {
        private List<string> droppedFiles = new List<string>();

        public event EventHandler AudioFilesProcessed;

        public FileDragDrop()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {

            if (tglbtn_audioTrack.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new AudioHandling.AudioTrack(droppedFiles[0], tbox_newFileName.Text));
            }
            else if (tglbtn_soundEffect.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new AudioHandling.SoundEffect(droppedFiles[0], tbox_newFileName.Text));
            }

            droppedFiles.RemoveAt(0);
            ProcessNextFile();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            droppedFiles.RemoveAt(0);
            ProcessNextFile();
        }

        public void OnDrop(List<string> filesDropped)
        {
            droppedFiles = filesDropped;
            ProcessNextFile();
        }

        private void ProcessNextFile()
        {
            if (droppedFiles.Count == 0)
            {
                AudioFilesProcessed?.Invoke(this, EventArgs.Empty);
                return;
            }

            string path = droppedFiles[0];
            if (!FileExtension.EndsWith(AppModel.Instance.ValidAudioExtensions, path))
            {
                droppedFiles.RemoveAt(0);
                ProcessNextFile();
                return;
            }

            if (GetSampleRate(path) != 44100)
            {
                // Ask for conversion
                // MessageBox.Nivisan();
                if (true)
                {
                    if (ConvertToSampleRate(ref path))
                        droppedFiles[0] = path;
                }
            }

            lbl_fileName.Content = Path.GetFileNameWithoutExtension(path);
            tbox_newFileName.Text = Path.GetFileNameWithoutExtension(path);

            lbl_filePath.Content = Path.GetDirectoryName(path);
        }

        private void SoundEffect_Clicked(object sender, RoutedEventArgs e)
        {
            if (tglbtn_soundEffect == null || tglbtn_audioTrack == null)
                return;

            tglbtn_audioTrack.IsChecked = !tglbtn_soundEffect.IsChecked;
        }

        private void AudioTrack_Clicked(object sender, RoutedEventArgs e)
        {
            if (tglbtn_soundEffect == null || tglbtn_audioTrack == null)
                return;

            tglbtn_soundEffect.IsChecked = !tglbtn_audioTrack.IsChecked;
        }

        private int GetSampleRate(string fileName)
        {
            WaveStream readerStream = null;
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new Mp3FileReader(fileName);
            }
            else if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                readerStream = new MediaFoundationReader(fileName);
            }

            int sampleRate = readerStream.WaveFormat.SampleRate;
            readerStream.Close();
            return sampleRate;
        }

        private bool ConvertToSampleRate(ref string fileName)
        {
            WaveStream readerStream = null;
            if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
            }
            else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new Mp3FileReader(fileName);
            }
            else if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
            {
                readerStream = new AiffFileReader(fileName);
            }
            else
            {
                // fall back to media foundation reader, see if that can play it
                readerStream = new MediaFoundationReader(fileName);
            }

            string outFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".wav");
            int outRate = 44100;
            if (readerStream.WaveFormat.SampleRate == outRate)
            {
                return false;
            }

            var outFormat = new WaveFormat(outRate, readerStream.WaveFormat.Channels);
            var resampler = new MediaFoundationResampler(readerStream, outFormat);
            try
            {
                WaveFileWriter.CreateWaveFile(outFile, resampler);
            }
            catch (Exception)
            {
                return false;
            }

            fileName = outFile;
            return true;
        }
    }
}
