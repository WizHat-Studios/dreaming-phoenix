using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace WizHat.DreamingPhoenix.Extensions
{
    public static class ProgressBarExtension
    {
        private static TimeSpan duration = TimeSpan.FromMilliseconds(250);

        public static void SetPercent(this ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new(percentage, duration);
            progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
        }
    }
}
