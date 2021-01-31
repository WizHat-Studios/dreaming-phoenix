using DreamingPhoenix.AudioHandling;
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

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for FileDragDrop.xaml
    /// </summary>
    public partial class FileDragDrop : UserControl, INotifyPropertyChanged
    {
        private List<string> droppedFiles = new List<string>();

        public event EventHandler AudioFilesProcessed;

        private string directoryPath;

        public string DirectoryPath
        {
            get { return directoryPath; }
            set
            {
                directoryPath = value;
                NotifyPropertyChanged();
            }
        }

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyPropertyChanged();
            }
        }

        private string audioName;

        public string AudioName
        {
            get { return audioName; }
            set
            {
                audioName = value;
                NotifyPropertyChanged();
            }
        }

        private string fileNameExt;

        public string FileNameExt
        {
            get { return fileNameExt; }
            set
            {
                fileNameExt = value;
                NotifyPropertyChanged();
            }
        }

        public FileDragDrop()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (tglbtn_audioTrack.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new AudioHandling.AudioTrack(droppedFiles[0], AudioName));
            }
            else if (tglbtn_soundEffect.IsChecked == true)
            {
                AppModel.Instance.AudioList.Add(new AudioHandling.SoundEffect(droppedFiles[0], AudioName));
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

            FileName = Path.GetFileNameWithoutExtension(path);
            AudioName = Path.GetFileNameWithoutExtension(path);
            FileNameExt = Path.GetFileName(path);
            //lbl_fileName.Content = Path.GetFileNameWithoutExtension(path);
            //lbl_fileName.ToolTip = Path.GetFileName(path);
            //lbl_fileNameConvert.Content = Path.GetFileNameWithoutExtension(path);
            //lbl_fileNameConvert.ToolTip = Path.GetFileName(path);
            //tbox_newFileName.Text = Path.GetFileNameWithoutExtension(path);

            DirectoryPath = Path.GetDirectoryName(path);
            //lbl_filePath.Content = Path.GetDirectoryName(path);
            //lbl_filePathConvert.Content = Path.GetDirectoryName(path);

            if (GetSampleRate(path) != 44100)
            {
                grd_audio.Visibility = Visibility.Hidden;
                grd_convert.Visibility = Visibility.Visible;
            }
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            droppedFiles.RemoveAt(0);

            // for better animation, only switch grid if not last file
            if (droppedFiles.Count != 0)
            {
                grd_audio.Visibility = Visibility.Visible;
                grd_convert.Visibility = Visibility.Hidden;
            }

            ProcessNextFile();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            pgb_converting.Visibility = Visibility.Visible;
            string path = droppedFiles[0];

            Task.Run(() =>
            {
                if (!ConvertToSampleRate(ref path))
                {
                    droppedFiles.RemoveAt(0);

                    pgb_converting.Dispatcher.Invoke(() => pgb_converting.Visibility = Visibility.Collapsed);

                    if (droppedFiles.Count != 0)
                    {
                        pgb_converting.Dispatcher.Invoke(() =>
                        {
                            grd_audio.Visibility = Visibility.Visible;
                            grd_convert.Visibility = Visibility.Hidden;
                        });
                    }

                    ProcessNextFile();
                    return;
                }

                droppedFiles[0] = path;
                FileName = Path.GetFileNameWithoutExtension(path);
                FileNameExt = Path.GetFileName(path);
                pgb_converting.Dispatcher.Invoke(() =>
                {
                    pgb_converting.Visibility = Visibility.Collapsed;
                    grd_audio.Visibility = Visibility.Visible;
                    grd_convert.Visibility = Visibility.Hidden;
                });
            });
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

            string outFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_Convert.wav");
            if (File.Exists(outFile))
            {
                MessageBox.Show(string.Format("File {0} already exists", outFile));
                return false;
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
