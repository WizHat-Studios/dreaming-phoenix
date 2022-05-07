using WizHat.DreamingPhoenix;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WizHat.DreamingPhoenix.HotkeyHandling.HotkeySelector
{
    public class HotkeySelector : Button
    {
        public static bool GlobalIsInSelectionModeLock = false;

        [Category("Common")]
        public Key SelectedHotkey
        {
            get => (Key)GetValue(SelectedHotkeyProperty);
            set => SetValue(SelectedHotkeyProperty, value);
        }

        public ModifierKeys ModifierKeys
        {
            get => (ModifierKeys)GetValue(ModifierKeysProperty);
            set => SetValue(ModifierKeysProperty, value);
        }
        public bool IsSelectingHotkey { get; set; } = false;

        public static readonly DependencyProperty SelectedHotkeyProperty = DependencyProperty.Register("SelectedHotkey", typeof(Key), typeof(HotkeySelector), new FrameworkPropertyMetadata(Key.NoName, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedHotkeyPropertyChanged));
        public static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register("ModifierKeys", typeof(ModifierKeys), typeof(HotkeySelector), new FrameworkPropertyMetadata(ModifierKeys.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnModifierKeysPropertyChanged));

        private string SelectedHotkeyText => (ModifierKeys.HasFlag(ModifierKeys.Shift) ? "Shift + " : "") +
                                             (ModifierKeys.HasFlag(ModifierKeys.Control) ? "Ctrl + " : "") +
                                             (ModifierKeys.HasFlag(ModifierKeys.Alt) ? "Alt + " : "") +
                                             SelectedHotkey.ToString();

        static HotkeySelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HotkeySelector), new FrameworkPropertyMetadata(typeof(HotkeySelector)));
            ContentProperty.OverrideMetadata(typeof(HotkeySelector), new FrameworkPropertyMetadata("None"));
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            IsSelectingHotkey = false;
            Content = SelectedHotkeyText;
            GlobalIsInSelectionModeLock = false;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (IsSelectingHotkey)
            {
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LWin || e.Key == Key.RWin)
                {
                    return;
                }

                SelectedHotkey = e.Key;
                ModifierKeys = Keyboard.Modifiers;
                IsSelectingHotkey = false;
                GlobalIsInSelectionModeLock = false;
            }
        }

        protected override void OnClick()
        {
            base.OnClick();
            IsSelectingHotkey = !IsSelectingHotkey;
            GlobalIsInSelectionModeLock = IsSelectingHotkey;
            if (IsSelectingHotkey)
            {
                Content = "Press Key...";
            }
            else
            {
                Content = SelectedHotkeyText;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            SelectedHotkey = Key.None;
            ModifierKeys = ModifierKeys.None;
        }

        private static void OnSelectedHotkeyPropertyChanged(DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            HotkeySelector hotkeySelector = (HotkeySelector)source;
            hotkeySelector.SelectedHotkey = (Key)e.NewValue;
            hotkeySelector.Content = hotkeySelector.SelectedHotkeyText;
        }

        private static void OnModifierKeysPropertyChanged(DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            HotkeySelector hotkeySelector = (HotkeySelector)source;
            hotkeySelector.ModifierKeys = (ModifierKeys)e.NewValue;
            hotkeySelector.Content = hotkeySelector.SelectedHotkeyText;
        }
    }
}
