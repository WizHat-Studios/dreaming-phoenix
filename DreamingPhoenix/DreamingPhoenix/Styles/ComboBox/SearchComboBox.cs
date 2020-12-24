using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;

namespace DreamingPhoenix.Styles.ComboBox
{
    public class SearchComboBox : System.Windows.Controls.ComboBox
    {
        private string oldFilter = string.Empty;
        private string currentFilter = string.Empty;

        protected TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;
        private List<object> list;
        private int oldSelectedIndex = -1;

        public SearchComboBox()
        {
            RequestBringIntoView += (s, e) =>
            {
                //Allows the keyboard to bring the items into view as expected:
                if (Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.Up))
                    return;

                e.Handled = true;
            };
            //EditableTextBox.Focus();
            GotFocus += (s, e) =>
            {
                EditableTextBox.Focus();
                if (EditableTextBox.Text == Text)
                {
                    EditableTextBox.Text = Text;
                    Text = "";
                }
            };
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue == null)
            {
                if (newValue == null || newValue.Cast<object>().Count() == 0)
                    return;
            }
            if (newValue == null)
            {
                if (oldValue.Cast<object>().Count() == 0)
                    return;
            }
            if (SameItems(oldValue, newValue))
                return;

            base.OnItemsSourceChanged(oldValue, newValue);

            if (oldValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(list);
                if (view != null)
                    view.Filter -= FilterItem;
            }

            if (newValue != null)
            {
                list = new List<object>(newValue.Cast<object>());
                var view = CollectionViewSource.GetDefaultView(list);
                view.Filter += FilterItem;
                ItemsSource = list;
            }
        }

        private bool SameItems(IEnumerable list1, IEnumerable list2)
        {
            if (list1 == null || list2 == null)
                return false;

            List<object> castList1 = list1.Cast<object>().ToList();
            List<object> castList2 = list2.Cast<object>().ToList();

            if (castList1.Count != castList2.Count)
                return false;

            for (int i = 0; i < castList1.Count; i++)
            {
                if (castList1[i] != castList2[i])
                    return false;
            }

            return true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                case Key.Enter:
                    IsDropDownOpen = false;
                    if (currentFilter != null && ItemWithExactText(currentFilter, out int index) != -1)
                        SelectedIndex = index;
                    Debug.WriteLine("Selected Index 1: " + SelectedIndex);
                    //if (currentFilter == "" && SelectedIndex != -1)
                    //    return;

                    Debug.WriteLine("Selected Index 2: " + SelectedIndex);
                    if (SelectedIndex != -1)
                    {
                        oldSelectedIndex = SelectedIndex;
                        return;
                    }

                    Debug.WriteLine("Selected Index 3: " + SelectedIndex);

                    SelectedIndex = 0;
                    currentFilter = "";
                    break;
                case Key.Escape:
                    IsDropDownOpen = false;
                    EditableTextBox.Text = "";
                    ClearFilter();
                    SelectedIndex = oldSelectedIndex;
                    break;
                default:
                    if (SelectedIndex != -1)
                        oldSelectedIndex = SelectedIndex;

                    if (e.Key == Key.Down) IsDropDownOpen = true;
                    base.OnPreviewKeyDown(e);
                    break;
            }

            // Cache text
            oldFilter = EditableTextBox.Text;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    break;
                case Key.Tab:
                case Key.Enter:
                    ClearFilter();
                    break;
                default:
                    if (SelectedIndex != -1)
                        oldSelectedIndex = SelectedIndex;

                    if (!IsDropDownRelevant(e.Key))
                        return;

                    if (EditableTextBox.Text != oldFilter)
                    {
                        var temp = EditableTextBox.Text;
                        RefreshFilter(); //RefreshFilter will change Text property
                        EditableTextBox.Text = temp;

                        if (SelectedIndex != -1 && EditableTextBox.Text != Items[SelectedIndex].ToString())
                        {
                            SelectedIndex = -1; //Clear selection. This line will also clear Text property
                            EditableTextBox.Text = temp;
                        }

                        IsDropDownOpen = true;
                        EditableTextBox.SelectionStart = int.MaxValue;
                    }

                    //automatically select the item when the input text matches it
                    //for (int i = 0; i < Items.Count; i++)
                    //{
                    //    if (EditableTextBox.Text == Items[i].ToString())
                    //        SelectedIndex = i;
                    //}

                    base.OnKeyUp(e);
                    currentFilter = EditableTextBox.Text;
                    break;
                    //if (string.IsNullOrEmpty(currentFilter))
                    //{
                    //    ClearFilter();
                    //    var temp = SelectedIndex;
                    //    SelectedIndex = -1;
                    //    Text = string.Empty;
                    //    SelectedIndex = temp;
                    //}
                    //break;
            }
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            //var temp = SelectedIndex;
            //SelectedIndex = -1;
            EditableTextBox.Text = string.Empty;
            //SelectedIndex = temp;
            base.OnPreviewLostKeyboardFocus(e);
        }

        private void RefreshFilter()
        {
            if (ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            view.Refresh();
        }

        private void ClearFilter()
        {
            currentFilter = string.Empty;
            EditableTextBox.Text = string.Empty;
            RefreshFilter();
        }

        private bool FilterItem(object value)
        {
            if (value == null) return false;
            if (EditableTextBox == null || EditableTextBox.Text.Length == 0) return true;

            return value.ToString().ToLower().Contains(EditableTextBox.Text.ToLower());
        }

        private int ItemWithExactText(string text, out int index)
        {
            index = -1;
            if (ItemsSource == null)
                return index;

            List<object> list = ItemsSource.Cast<object>().ToList();
            index = list.FindIndex(l => l.ToString().ToLower() == text.ToLower());
            return index;
        }

        private bool IsDropDownRelevant(Key key)
        {
            if (IsLetterOrDigit(key))
                return true;

            if (key == Key.Back)
                return true;

            if (key == Key.OemMinus)
                return true;

            return false;
        }

        private bool IsLetterOrDigit(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
                return true;

            if (key >= Key.D0 && key <= Key.D9)
                return true;

            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                return true;

            return false;
        }
    }
}
