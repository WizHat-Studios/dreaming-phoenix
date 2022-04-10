using System.Windows;

namespace WizHat.DreamingPhoenix.Styles
{
    public partial class CustomWindow : ResourceDictionary
    {
        public CustomWindow()
        {
            InitializeComponent();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void MaximizeRestoreClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            if (window.WindowState == WindowState.Normal)
                window.WindowState = WindowState.Maximized;
            else
                window.WindowState = WindowState.Normal;
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        private void OnResize(object sender, RoutedEventArgs e)
        {
            var window = (Window)sender;

            if (window == null) return;

            if (window.WindowState == WindowState.Maximized)
            {
                window.BorderThickness = new Thickness(7);
                window.Margin = new Thickness(7);
            }
            else
            {
                window.BorderThickness = new Thickness(0);
            }
        }
    }
}