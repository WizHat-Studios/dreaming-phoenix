using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.AudioProperties;
using WizHat.DreamingPhoenix.Data;
using WizHat.DreamingPhoenix.Extensions;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for ItemSelectionList.xaml
    /// </summary>
    public partial class ItemSelectionList : DialogControl
    {
        #region Page Texts
        private string itemTitle;
        public string PageTitle
        {
            get { return $"Select {itemTitle}".ToUpper(); }
        }

        public string NoItemsText
        {
            get { return $"Seems like there isn't any {itemTitle} to select from here".ToUpper(); }
        }

        public string NoItemsFoundText
        {
            get { return $"Oops, looks like there is no {itemTitle} for that".ToUpper(); }
        }

        public string NoItemsSelectedText
        {
            get { return $"No {itemTitle} selected"; }
        }
        #endregion

        #region Selection List
        private IEnumerable<object> selectionList;
        public IEnumerable<object> SelectionList
        {
            get { return selectionList; }
            set
            {
                selectionList = value;
                NotifyPropertyChanged();
            }
        }

        private IEnumerable<object> filteredSelectionList;
        public IEnumerable<object> FilteredSelectionList
        {
            get { return filteredSelectionList; }
            set
            {
                filteredSelectionList = value;
                NotifyPropertyChanged();
            }
        }

        private IEnumerable<object> selectedItems = new List<object>();
        public IEnumerable<object> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                selectedItems = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SelectedText));
            }
        }

        private bool multiSelection;
        public bool MultiSelection
        {
            get { return multiSelection; }
            set
            {
                multiSelection = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SelectionMode));
            }
        }

        public SelectionMode SelectionMode
        {
            get
            {
                if (multiSelection)
                    return SelectionMode.Multiple;

                return SelectionMode.Single;
            }
        }
        #endregion

        #region Search
        private string searchText = "";
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SearchActive));
                FilterList();
            }
        }

        public bool SearchActive { get { return !string.IsNullOrEmpty(SearchText); } }
        #endregion

        public string SelectedText
        {
            get
            {
                if (SelectedItems.Count() == 0)
                    return NoItemsSelectedText;

                string selectedItems = string.Join(',', SelectedItems);
                return selectedItems.Replace(",", ", ");
            }
        }

        #region Events
        public event EventHandler<string> OnAddItem;
        public event EventHandler<object> OnRemoveItem;
        public event ReturnEventHandler<IEnumerable<object>> OnGetSourceList;
        #endregion

        private ItemSelectionList()
        {
            InitializeComponent();
            DataContext = this;

            ((INotifyCollectionChanged)lbox_selectionList.Items).CollectionChanged += DetermineListPrompt;
            Loaded += (s, e) =>
            {
                FilterList();

                // Select previous items
                foreach (object item in SelectedItems)
                {
                    lbox_selectionList.SelectedItems.Add(item);
                }
            };
        }

        public ItemSelectionList(string itemTitle, List<Audio> currentAudios, Type audioType = null, List<Audio> selectionList = null, bool multiSelection = false) : this()
        {
            this.itemTitle = itemTitle;
            MultiSelection = multiSelection;
            SelectedItems = currentAudios;

            if (selectionList is null)
                SelectionList = AppModel.Instance.AudioList;
            else
                SelectionList = selectionList;

            if (audioType is not null)
                SelectionList = SelectionList.Where(a => a.GetType() == audioType);
        }

        public ItemSelectionList(string itemTitle, List<Category> currentCategories, List<Category> selectionList = null, bool multiSelection = false) : this()
        {
            this.itemTitle = itemTitle;
            MultiSelection = multiSelection;
            SelectedItems = currentCategories;

            if (selectionList is null)
                SelectionList = AppModel.Instance.Categories;
            else
                SelectionList = selectionList;
        }

        public ItemSelectionList(string itemTitle, List<Tag> currentTags, List<Tag> selectionList = null, bool multiSelection = false) : this()
        {
            this.itemTitle = itemTitle;
            MultiSelection = multiSelection;
            SelectedItems = currentTags;

            if (selectionList is null)
                SelectionList = AppModel.Instance.AvailableTags;
            else
                SelectionList = selectionList;
        }

        private void FilterList()
        {
            SelectionList = OnGetSourceList(this, EventArgs.Empty);
            FilteredSelectionList = SelectionList.Where(x => x.ToString().ToLower().Contains(SearchText.ToLower()));
        }

        private void Search_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnAddItem?.Invoke(this, SearchText);
                FilterList();
            }
        }

        private void DetermineListPrompt(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (FilteredSelectionList.Count() == 0)
            {
                if (SelectionList.Count() == 0)
                {
                    grid_emptyListboxPrompt.Visibility = Visibility.Visible;
                    grid_noSearchListboxPrompt.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_emptyListboxPrompt.Visibility = Visibility.Collapsed;
                    grid_noSearchListboxPrompt.Visibility = Visibility.Visible;
                }
            }
            else
            {
                grid_emptyListboxPrompt.Visibility = Visibility.Collapsed;
                grid_noSearchListboxPrompt.Visibility = Visibility.Collapsed;
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            OnAddItem?.Invoke(this, SearchText);
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveItem?.Invoke(this, ((ListBoxItem)sender).DataContext);
        }

        private void lbox_selectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems = lbox_selectionList.SelectedItems.Cast<object>().ToList();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Deactivated when multiSelection is enabled
            if (multiSelection)
                return;

            Close(new List<object> { ((ListBoxItem)sender).DataContext });
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close(SelectedItems);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close(new List<object>());
        }
    }
}
