using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        // Saturation from 0 to 255
        public byte CurrentSaturation { get; set; }

        // Lightness from 0 to 255
        public byte CurrentLightness { get; set; }

        // Hue from 0 to 1
        private double CurrentHue { get; set; }

        private Color oldColor = Colors.White;
        public Color OldColor
        {
            get { return oldColor; }
            set { oldColor = value; NotifyPropertyChanged(); }
        }

        private Color newColor;
        public Color NewColor
        {
            get { return newColor; }
            set 
            { 
                newColor = value; 
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Color> predefinedColors = new ObservableCollection<Color>()
        {
            GetColorFromHex("#FFFFBE"),
            GetColorFromHex("#FFFE4F"),
            GetColorFromHex("#FFFE06"),

            GetColorFromHex("#FEE7BC"),
            GetColorFromHex("#FFC250"),
            GetColorFromHex("#FDA602"),

            GetColorFromHex("#F7D8C9"),
            GetColorFromHex("#E99A60"),
            GetColorFromHex("#E06F21"),

            GetColorFromHex("#FFBFC1"),
            GetColorFromHex("#FE4C4B"),
            GetColorFromHex("#FF0502"),

            GetColorFromHex("#FFB4C0"),
            GetColorFromHex("#FF6683"),
            GetColorFromHex("#FF1A41"),

            GetColorFromHex("#FFC1FD"),
            GetColorFromHex("#FF4CFF"),
            GetColorFromHex("#FF00FE"),

            GetColorFromHex("#E1CAF9"),
            GetColorFromHex("#A561E8"),
            GetColorFromHex("#8A2AE5"),

            GetColorFromHex("#C4C4FF"),
            GetColorFromHex("#504CFF"),
            GetColorFromHex("#1700FF"),

            GetColorFromHex("#C2FEC3"),
            GetColorFromHex("#4FFE4F"),
            GetColorFromHex("#02FE05")
        };
        public ObservableCollection<Color> PredefinedColors
        {
            get { return predefinedColors; }
            set { predefinedColors = value; NotifyPropertyChanged(); }
        }

        public ColorPicker()
        {
            InitializeComponent();
            this.DataContext = this;
            OldColor = Colors.Red;
            NewColor = OldColor;
            SetHSVFromColor(NewColor);
            
        }

        public ColorPicker(Color oldColor)
        {
            InitializeComponent();
            this.DataContext = this;
            OldColor = oldColor;
            NewColor = OldColor;
            SetHSVFromColor(NewColor);
        }

        public void PickNewColor(Color oldColor)
        {
            OldColor = oldColor;
            NewColor = oldColor;
            SetHSVFromColor(OldColor);
        }

        private static Color GetColorFromHex(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = Mouse.GetPosition((Rectangle)sender);
            UpdateColorRectangle(ClampByte(point.X), ClampByte(point.Y));
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                Point point = Mouse.GetPosition((Rectangle)sender);
                UpdateColorRectangle(ClampByte(point.X), ClampByte(point.Y));
            }
        }
        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point point = Mouse.GetPosition((Rectangle)sender);
                UpdateColorRectangle(ClampByte(point.X), ClampByte(point.Y));
            }
        }

        private byte ClampByte(double doubleToClamp)
        {
            if (doubleToClamp < 0)
                return (byte)0;
            else if (doubleToClamp > 255)
                return (byte)255;
            return (byte)doubleToClamp;
        }

        private void UpdateColorRectangle(byte saturation, byte lightness)
        {
            CurrentSaturation = saturation; 
            CurrentLightness = lightness;

            brd_mouse.Margin = new Thickness(saturation, lightness, 0, 0);
            double l = ((lightness / 255.0) - 1.0) * -1;
            NewColor = ConvertHSVToRGB(CurrentHue, saturation / 255.0, l);

            if ((NewColor.R * 0.299 + NewColor.G * 0.587 + NewColor.B * 0.114) > 149)
            {
                brd_mouse.Stroke = new SolidColorBrush(Colors.Black);
            }
            else
            {
                brd_mouse.Stroke = new SolidColorBrush(Colors.White);
            }

            lbl_newHexColorText.Content = HexConverter(NewColor);

        }

        public void SetHSVFromColor(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            double hue = drawingColor.GetHue() / 360;
            byte saturation = (byte)(((max == 0) ? 0 : 1d - (1d * min / max)) * 255);
            byte value = (byte)max;

            CurrentHue = hue;
            CurrentLightness = (byte)Math.Abs(value - 255);
            CurrentSaturation = saturation;

            UpdateHue(CurrentHue);
            UpdateColorRectangle(CurrentSaturation, CurrentLightness);
        }

        private static String HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private Color ConvertHSVToRGB(double h, double s, double v)
        {
            double r, g, b, i, f, p, q, t;
            r = 0;
            g = 0;
            b = 0;

            i = Math.Floor(h * 6);
            f = h * 6 - i;
            p = v * (1 - s);
            q = v * (1 - f * s);
            t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return Color.FromScRgb(1, (float)r, (float)g, (float)b);

        }

        private void rect_hue_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {

                Point point = Mouse.GetPosition((Rectangle)sender);
                UpdateHue(point.Y / 255.0);
            }
        }

        private void rect_hue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = Mouse.GetPosition((Rectangle)sender);
            UpdateHue(point.Y / 255.0);
        }

        private void UpdateHue(double newHue)
        {
            brd_huePos.Margin = new Thickness(2, newHue * 255, 2, 0);
            CurrentHue = newHue;
            this.Resources["CurrentColor"] = GetColorForHueStep(CurrentHue);
            UpdateColorRectangle(CurrentSaturation, CurrentLightness);
        }

        private Color GetColorForHueStep(double step)
        {
            if (step == 1)
                step = 0;

            double t = step % (1.0 / 6);
            byte i = (byte)(255 / (1.0 / 6) * t);

            if (step < 0.166666666666)
                return Color.FromRgb(255, i, 0);
            if (step < 0.333333333333)
                return Color.FromRgb((byte)(255 - i), 255, 0);
            if (step < 0.5)
                return Color.FromRgb(0, 255, i);
            if (step < 0.666666666666)
                return Color.FromRgb(0, (byte)(255 - i), 255);
            if (step < 0.833333333333)
                return Color.FromRgb(i, 0, 255);
            if (step <= 1.0)
                return Color.FromRgb(255, 0, (byte)(255 - i));

            return Colors.Black;
        }

        private void PredefinedColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetHSVFromColor((Color)(sender as Ellipse).Tag);
        }

        private void ResetColor_Click(object sender, RoutedEventArgs e)
        {
            NewColor = OldColor;
            SetHSVFromColor(NewColor);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
