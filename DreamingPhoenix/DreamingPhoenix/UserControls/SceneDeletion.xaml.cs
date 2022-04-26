using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SceneDeletion.xaml
    /// </summary>
    public partial class SceneDeletion : DialogControl, INotifyPropertyChanged
    {
        private Scene sceneToDelete;

        public Scene SceneToDelete
        {
            get { return sceneToDelete; }
            set
            {
                sceneToDelete = value;
                NotifyPropertyChanged();
            }
        }

        public SceneDeletion(Scene sceneToDelete)
        {
            InitializeComponent();
            DataContext = this;
            SceneToDelete = sceneToDelete;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await Delete(sceneToDelete);
            Close();
        }

        private async Task Delete(Scene sceneToDelete)
        {
            AppModel.Instance.SceneList.Remove(sceneToDelete);
            AppModel.Instance.SaveData();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
