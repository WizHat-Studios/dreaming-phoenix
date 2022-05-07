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
        // Hue from 0 to 360
        private int CurrentHue { get; set; }

        // Saturation from 0 to 1
        public double CurrentSaturation { get; set; }

        // Lightness from 0 to 1
        public double CurrentValue { get; set; }

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
            ConvertHexToColor("#FFFFBE"),
            ConvertHexToColor("#FFFE4F"),
            ConvertHexToColor("#FFFE06"),

            ConvertHexToColor("#FEE7BC"),
            ConvertHexToColor("#FFC250"),
            ConvertHexToColor("#FDA602"),

            ConvertHexToColor("#F7D8C9"),
            ConvertHexToColor("#E99A60"),
            ConvertHexToColor("#E06F21"),

            ConvertHexToColor("#FFBFC1"),
            ConvertHexToColor("#FE4C4B"),
            ConvertHexToColor("#FF0502"),

            ConvertHexToColor("#FFB4C0"),
            ConvertHexToColor("#FF6683"),
            ConvertHexToColor("#FF1A41"),

            ConvertHexToColor("#FFC1FD"),
            ConvertHexToColor("#FF4CFF"),
            ConvertHexToColor("#FF00FE"),

            ConvertHexToColor("#E1CAF9"),
            ConvertHexToColor("#A561E8"),
            ConvertHexToColor("#8A2AE5"),

            ConvertHexToColor("#C4C4FF"),
            ConvertHexToColor("#504CFF"),
            ConvertHexToColor("#1700FF"),

            ConvertHexToColor("#C2FEC3"),
            ConvertHexToColor("#4FFE4F"),
            ConvertHexToColor("#02FE05")
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

        private void UpdateHue(int newHue)
        {
            CurrentHue = newHue;
            brd_huePanelSelector.Margin = new Thickness(2, newHue * rec_huePanel.ActualHeight / 360, 2, 0);
            Resources["CurrentColor"] = GetColorForHueStep(CurrentHue / 360.0);
            UpdateSaturationValuePanel(CurrentSaturation, CurrentValue);
        }

        private void UpdateSaturationValuePanel(double saturation, double value)
        {
            if (saturation < 0)
                saturation = 0;
            else if (saturation > 1)
                saturation = 1;

            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;

            CurrentSaturation = saturation; 
            CurrentValue = value;

            path_colorSelector.Margin = new Thickness(saturation * rec_saturationValuePanel.ActualWidth, Math.Abs(value - 1) * rec_saturationValuePanel.ActualHeight, 0, 0);
            NewColor = ConvertHSVToRGB(CurrentHue, CurrentSaturation, CurrentValue);

            if ((NewColor.R * 0.299 + NewColor.G * 0.587 + NewColor.B * 0.114) > 149)
                path_colorSelector.Stroke = new SolidColorBrush(Colors.Black);
            else
                path_colorSelector.Stroke = new SolidColorBrush(Colors.White);

            tbx_hexColor.Text = ConvertColorToHex(NewColor);
        }

        public void SetHSVFromColor(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            CurrentHue = (int)drawingColor.GetHue();
            CurrentSaturation = (max == 0) ? 0 : 1d - (1d * min / max);
            CurrentValue = max / 255.0;

            UpdateHue(CurrentHue);
            UpdateSaturationValuePanel(CurrentSaturation, CurrentValue);
        }

        private static Color ConvertHexToColor(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private static string ConvertColorToHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private Color ConvertHSVToRGB(int hue, double s, double v)
        {
            double h = hue / 360.0;
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

            return Color.FromRgb((byte)Math.Round(r * 255), (byte)Math.Round(g * 255), (byte)Math.Round(b * 255));
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

        private void ResetColor_Click(object sender, RoutedEventArgs e)
        {
            NewColor = OldColor;
            SetHSVFromColor(NewColor);
        }

        private void tbx_hexColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbx_hexColor.Text == ConvertColorToHex(NewColor))
                return;

            if (tbx_hexColor.Text.StartsWith("#") && tbx_hexColor.Text.Length != 7)
                return;

            if (!tbx_hexColor.Text.StartsWith("#") && tbx_hexColor.Text.Length != 6)
                return;

            if (tbx_hexColor.Text.Length == 6)
                tbx_hexColor.Text = "#" + tbx_hexColor.Text;

            NewColor = ConvertHexToColor(tbx_hexColor.Text);
            SetHSVFromColor(NewColor);
        }

        private void rec_huePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {

                Point point = Mouse.GetPosition((Rectangle)sender);
                UpdateHue((int)Math.Round(point.Y / rec_huePanel.ActualHeight * 360));
            }
        }

        private void rec_huePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = Mouse.GetPosition((Rectangle)sender);
            UpdateHue((int)Math.Round(point.Y / rec_huePanel.ActualHeight * 360));
        }

        private void PredefinedColor_Click(object sender, RoutedEventArgs e)
        {
            NewColor = (Color)(sender as Button).Tag;
            SetHSVFromColor(NewColor);
        }

        private void rec_saturationValuePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = Mouse.GetPosition((Rectangle)sender);
            UpdateSaturationValuePanel(point.X / ((Rectangle)sender).ActualWidth, Math.Abs((point.Y / ((Rectangle)sender).ActualHeight) - 1.0));
        }

        private void rec_saturationValuePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                Point point = Mouse.GetPosition((Rectangle)sender);
                UpdateSaturationValuePanel(point.X / ((Rectangle)sender).ActualWidth, Math.Abs((point.Y / ((Rectangle)sender).ActualHeight) - 1.0));
            }
        }
        private void rec_saturationValuePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point point = Mouse.GetPosition((Rectangle)sender);
                UpdateSaturationValuePanel(point.X / ((Rectangle)sender).ActualWidth, Math.Abs((point.Y / ((Rectangle)sender).ActualHeight) - 1.0));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
