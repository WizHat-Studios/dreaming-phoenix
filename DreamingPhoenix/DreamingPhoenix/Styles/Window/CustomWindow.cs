﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DreamingPhoenix.Styles
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
            if (window.WindowState == System.Windows.WindowState.Normal)
                window.WindowState = System.Windows.WindowState.Maximized;
            else
                window.WindowState = System.Windows.WindowState.Normal;
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = System.Windows.WindowState.Minimized;
        }

        private void OnResize(object sender, RoutedEventArgs e)
        {
            var window = (Window)(sender);

            if (window == null) return;


            if (window.WindowState == WindowState.Maximized)
            {
                window.BorderThickness = new Thickness(8);
                window.Margin = new Thickness(8);
            }
            else
            {
                window.BorderThickness = new Thickness(0);
            }
        }
    }
}
