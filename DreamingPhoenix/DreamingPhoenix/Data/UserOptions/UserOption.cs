using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public abstract class UserOption : Control, INotifyPropertyChanged
    {
        private UserOptionCategory category;
        public UserOptionCategory Category
        {
            get { return category; }
            set
            {
                category = value;
                NotifyPropertyChanged();
            }
        }

        private string optionName;
        public string OptionName
        {
            get { return optionName; }
            set
            {
                optionName = value.ToUpper();
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
