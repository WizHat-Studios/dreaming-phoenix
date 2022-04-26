using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WizHat.DreamingPhoenix.Data.UserOptions
{
    public class UserHotKeyOption : UserOption
    {
        public Key SelectedHotKey
        {
            get { return (Key)GetValue(SelectedHotKeyProperty); }
            set { SetValue(SelectedHotKeyProperty, value); }
        }

        public static readonly DependencyProperty SelectedHotKeyProperty =
            DependencyProperty.Register(nameof(SelectedHotKey), typeof(Key), typeof(UserHotKeyOption), new FrameworkPropertyMetadata(Key.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)GetValue(ModifierKeysProperty); }
            set { SetValue(ModifierKeysProperty, value); }
        }

        public static readonly DependencyProperty ModifierKeysProperty =
            DependencyProperty.Register(nameof(ModifierKeys), typeof(ModifierKeys), typeof(UserHotKeyOption), new FrameworkPropertyMetadata(ModifierKeys.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
