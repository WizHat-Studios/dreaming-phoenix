using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public class UserActionOption : UserOption
    {
        private string actionText;
        public string ActionText
        {
            get { return actionText; }
            set
            {
                actionText = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ActionCommand
        {
            get { return (ICommand)GetValue(ActionCommandProperty); }
            set
            {
                SetValue(ActionCommandProperty, value);
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ActionCommandProperty =
            DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(UserActionOption), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
