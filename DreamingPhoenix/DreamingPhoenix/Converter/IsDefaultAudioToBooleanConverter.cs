using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using WizHat.DreamingPhoenix.AudioHandling;

namespace WizHat.DreamingPhoenix.Converter
{
    public class IsDefaultAudioToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(AudioTrack))
            {
                if ((AudioTrack)value == AudioTrack.Default)
                    return false;
            }
            else if (value.GetType() == typeof(SoundEffect))
            {
                if ((SoundEffect)value == SoundEffect.Default)
                    return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
