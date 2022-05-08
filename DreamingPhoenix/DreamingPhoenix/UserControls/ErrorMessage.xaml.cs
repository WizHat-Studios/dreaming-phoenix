using System;
using System.Collections.Generic;
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

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : DialogControl, INotifyPropertyChanged
    {
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }


        public ErrorMessage(string message, string title = "THAT DIDN'T WORK OUT. WE'RE SORRY...", MessageBoxButton messageBoxButton = MessageBoxButton.OK)
        {
            InitializeComponent();
            this.DataContext = this;
            Message = message;

            lbl_title.Content = title;

            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    btn_ok.Visibility = Visibility.Visible;
                    btn_cancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    btn_ok.Visibility = Visibility.Visible;
                    btn_cancel.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close(MessageBoxResult.OK);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(MessageBoxResult.Cancel);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
