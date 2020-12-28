﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for UserApplicationOptions.xaml
    /// </summary>
    public partial class UserApplicationOptions : UserControl
    {

        public AppOptions Options { get; set; } = AppModel.Instance.Options;

        public UserApplicationOptions()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}