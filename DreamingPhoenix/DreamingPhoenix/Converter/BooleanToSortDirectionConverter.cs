using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WizHat.DreamingPhoenix.Sorting;

namespace WizHat.DreamingPhoenix.Converter
{
    class BooleanToSortDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((SortDirection)value == SortDirection.DESCENDING)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return SortDirection.DESCENDING;
            }
            return SortDirection.ASCENDING;
        }
    }
}
