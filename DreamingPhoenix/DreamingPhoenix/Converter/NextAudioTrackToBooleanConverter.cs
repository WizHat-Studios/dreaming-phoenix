﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using WizHat.DreamingPhoenix.AudioHandling;

namespace WizHat.DreamingPhoenix.Converter
{
    public class NextAudioTrackToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(AudioTrack))
                throw new NotSupportedException("Value must be a AudioTrack");

            if (((AudioTrack)value).NextAudioTrack == null || !((AudioTrack)value).NextAudioTrack.IsAudioFilePathValid)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
