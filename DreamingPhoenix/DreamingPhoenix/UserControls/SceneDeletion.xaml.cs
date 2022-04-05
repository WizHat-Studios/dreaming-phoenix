﻿using DreamingPhoenix.AudioHandling;
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

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for SceneDeletion.xaml
    /// </summary>
    public partial class SceneDeletion : UserControl, INotifyPropertyChanged
    {
        public event EventHandler OperationProcessed;

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

        public SceneDeletion()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void AcceptDelete(Scene sceneToDelete)
        {
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
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        private async Task Delete(Scene sceneToDelete)
        {
            AppModel.Instance.SceneList.Remove(sceneToDelete);
            AppModel.Instance.SaveData();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }
    }
}
