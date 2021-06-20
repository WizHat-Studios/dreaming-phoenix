using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for FilterSettings.xaml
    /// </summary>
    public partial class FilterSettings : UserControl, INotifyPropertyChanged
    {
        public event EventHandler OperationProcessed;
        public event PropertyChangedEventHandler PropertyChanged;

        public FilterOptions NewFilterOptions { get; set; } = (FilterOptions)AppModel.Instance.Options.FilterOptions.Clone();

        public FilterSettings()
        {
            InitializeComponent();
            AppModel.Instance.UpdateAvailableTags();
            AppModel.Instance.UpdateAvailableCategories();
            NewFilterOptions.UpdateTags();
            DataContext = this;
        }

        private void btn_apply_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.Options.FilterOptions = (FilterOptions)NewFilterOptions.Clone();
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            OperationProcessed?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
