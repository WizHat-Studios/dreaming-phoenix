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

        public bool Equals(Tag other)
        {
            return other.Text == Text;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Tag)
                return false;

            return Equals((Tag)obj);
        }

        public override string ToString()
        {
            return Text;
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
