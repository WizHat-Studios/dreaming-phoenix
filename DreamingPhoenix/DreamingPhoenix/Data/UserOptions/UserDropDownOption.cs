using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public class UserDropDownOption : UserBindingOption<int>
    {
        public ObservableCollection<object> List
        {
            get { return (ObservableCollection<object>)GetValue(ListProperty); }
            set
            {
                SetValue(ListProperty, value);
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ListProperty =
            DependencyProperty.Register(nameof(List), typeof(ObservableCollection<object>), typeof(UserDropDownOption), new FrameworkPropertyMetadata(new ObservableCollection<object>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
