using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.Extensions
{
    internal static class HelperFunctions
    {
        public static bool IsNullOrEmpty(ICollection list)
        {
            if (list is null)
                return true;

            if (list.Count == 0)
                return true;

            return false;
        }

        public static bool IsNullOrEmpty(IEnumerable list)
        {
            return IsNullOrEmpty((ICollection)list);
        }

        public static void RefreshAudioListView()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ApplyFilterOptions(AppModel.Instance.Options.FilterOptions);
        }
    }
}
