using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WizHat.DreamingPhoenix.AudioProperties
{
    public class Tag : INotifyPropertyChanged, IEquatable<Tag>
    {
        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; NotifyPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public bool Equals(Tag other)
        {
            return other.Text == Text;
        }

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SelectableTag : Tag
    {
        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set { selected = value; NotifyPropertyChanged(); }
        }
    }
}
