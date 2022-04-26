using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public abstract class UserBindingOption<T> : UserOption
    {
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(T), typeof(UserBindingOption<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
