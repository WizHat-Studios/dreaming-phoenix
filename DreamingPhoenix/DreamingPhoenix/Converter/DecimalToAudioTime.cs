using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace DreamingPhoenix.Converter
{
    public class DecimalToAudioTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double minutes = 0;
            double seconds = (double)value;
            if (seconds >= 60)
            {
                double mod = seconds % 60;
                minutes = (seconds - mod) / 60;
            }
            seconds = seconds - (minutes * 60);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
