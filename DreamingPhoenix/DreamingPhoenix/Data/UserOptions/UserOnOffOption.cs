using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public class UserOnOffOption : UserBindingOption<bool>
    {
        private bool invert;
        public bool Invert
        {
            get { return invert; }
            set
            {
                invert = value;
                NotifyPropertyChanged();
            }
        }
    }
}
