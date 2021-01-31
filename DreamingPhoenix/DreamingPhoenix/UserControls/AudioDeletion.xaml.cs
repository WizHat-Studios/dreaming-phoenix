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
    public partial class FileDeletion : UserControl, INotifyPropertyChanged
    {
        public event EventHandler OperationProcessed;

        private Audio audioToDelete;

        public Audio AudioToDelete
        {
            get { return audioToDelete; }
            set
            {
                audioToDelete = value;
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

        public FileDeletion()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void AcceptDelete(Audio audioToDelete)
        {
            AudioToDelete = audioToDelete;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Delete(audioToDelete);
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteWithoutConfirmation(Audio audioToDelete)
        {
            Delete(audioToDelete);
        }

        private void Delete(Audio audioToDelete)
        {
            AppModel.Instance.AudioList.Remove(audioToDelete);
            AppModel.Instance.SaveData();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }
    }
}
