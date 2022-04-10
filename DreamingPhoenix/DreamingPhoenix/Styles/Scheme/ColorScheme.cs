using System;
using System.Collections.Generic;
using System.Text;

namespace WizHat.DreamingPhoenix.Styles.Scheme
{
    public class ColorScheme
    {
        public Uri ThemeDestination { get; set; }
        public string ThemeDisplayName { get; set; }

        public static readonly List<ColorScheme> Themes = new List<ColorScheme>()
        {
            new ColorScheme("Dreamland", new Uri("./Styles/Scheme/ColorSchemeDreamland.xaml", UriKind.Relative)),
            new ColorScheme("Phoenix", new Uri("./Styles/Scheme/ColorSchemePhoenix.xaml", UriKind.Relative)),
            new ColorScheme("Winter Wolf", new Uri("./Styles/Scheme/ColorSchemeWinterWolf.xaml", UriKind.Relative))
        };


        public ColorScheme(string themeDisplayName, Uri themeDestination)
        {
            ThemeDestination = themeDestination;
            ThemeDisplayName = themeDisplayName;
        }
    }
}
