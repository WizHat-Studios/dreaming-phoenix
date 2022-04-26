using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WizHat.DreamingPhoenix.AudioProperties
{
    public class Category : INotifyPropertyChanged, IEquatable<Category>
    {
        private static Category defaultCategory = new Category() { Name = "None" };
        public static Category Default { get { return defaultCategory; } }

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

        public bool Equals(Category other)
        {
            return other.Name == Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
