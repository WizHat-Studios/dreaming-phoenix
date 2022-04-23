using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WizHat.DreamingPhoenix.Data
{
    public class WindowOptions : INotifyPropertyChanged
    {
        private double height = 500;
        public double Height
        {
            get { return height; }
            set { height = value; NotifyPropertyChanged(); }
        }

        private double width = 700;
        public double Width
        {
            get { return width; }
            set { width = value; NotifyPropertyChanged(); }
        }

        private GridLength gridSplitterVerticalColumnOne = new GridLength(2, GridUnitType.Star);
        public GridLength GridSplitterVerticalColumnOne
        {
            get { return gridSplitterVerticalColumnOne; }
            set { gridSplitterVerticalColumnOne = value; NotifyPropertyChanged(); }
        }

        private GridLength gridSplitterVerticalColumnThree = new GridLength(1, GridUnitType.Star);
        public GridLength GridSplitterVerticalColumnThree
        {
            get { return gridSplitterVerticalColumnThree; }
            set { gridSplitterVerticalColumnThree = value; NotifyPropertyChanged(); }
        }

        private GridLength gridSplitterHorizontallRowOne = new GridLength(1, GridUnitType.Star);
        public GridLength GridSplitterHorizontallRowOne
        {
            get { return gridSplitterHorizontallRowOne; }
            set { gridSplitterHorizontallRowOne = value; NotifyPropertyChanged(); }
        }

        private GridLength gridSplitterHorizontallRowThree = new GridLength(1, GridUnitType.Star);
        public GridLength GridSplitterHorizontallRowThree
        {
            get { return gridSplitterHorizontallRowThree; }
            set { gridSplitterHorizontallRowThree = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
