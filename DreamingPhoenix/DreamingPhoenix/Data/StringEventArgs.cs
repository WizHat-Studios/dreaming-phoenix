using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizHat.DreamingPhoenix.Data
{
    public class StringEventArgs : EventArgs
    {
        private string value;
        public string Value
        {
            get { return value; }
            private set { this.value = value; }
        }

        public StringEventArgs(string value)
        {
            Value = value;
        }
    }
}
