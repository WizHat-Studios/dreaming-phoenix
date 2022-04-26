using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizHat.DreamingPhoenix.Data
{
    public class DoubleEventArgs : EventArgs
    {
        private readonly double value;
        public double Value
        {
            get { return value; }
        }

        public DoubleEventArgs(double value)
        {
            this.value = value;
        }
    }
}
