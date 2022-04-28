using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WizHat.DreamingPhoenix.AudioProperties
{
    //[DebuggerDisplay("Name: {Name} - Color: {Color}")]
    public class Category : INotifyPropertyChanged, IEquatable<Category>
    {
        public static Category Default
        {
            get
            {
                return new Category("None");
            }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(Color)); }
        }

        public Color Color
        {
            get
            {
                string hash = BitConverter.ToUInt32(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(Name)), 0).ToString();
                return (Color)ColorConverter.ConvertFromString("#" + hash.Substring(0, 6));
            }
        }

        public Category(string name)
        {
            Name = name;
        }

        public bool IsDefault()
        {
            return Equals(Default);
        }

        public bool Equals(Category other)
        {
            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Category)
                return false;

            return Equals((Category)obj);
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
