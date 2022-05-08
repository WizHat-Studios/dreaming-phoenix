using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            get { return $"No {itemTitle.ToLower()} selected"; }
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

                if (SelectionList is null)
                    return;

                SelectionListCollectionView = CollectionViewSource.GetDefaultView(SelectionList);
                SelectionListCollectionView.Filter = FilterItem;
                NotifyPropertyChanged(nameof(SelectionListCollectionView));
            }
        }

        private List<object> selectedItems = new List<object>();
        public List<object> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                if (value is null)
                    selectedItems = new List<object>();
                else
                    selectedItems = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SelectedText));
            }
        }

        public ICollectionView SelectionListCollectionView { get; set; }

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

        private IEnumerable<object> previousItems;
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
        public Func<object, string, bool> SearchItem { get; set; }
        #endregion

        #region Options
        private bool isAddEnabled;
        public bool IsAddEnabled
        {
            get { return isAddEnabled; }
            set
            {
                isAddEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool isRemoveEnabled;
        public bool IsRemoveEnabled
        {
            get { return isRemoveEnabled; }
            set
            {
                isRemoveEnabled = value;
                NotifyPropertyChanged();
            }
        }
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

        #region Category
        private Category categoryToSelectColor = null;
        #endregion

        private const string AUDIO_TITLE = "Audio";
        private const string CATEGORY_TITLE = "Category";
        private const string TAG_TITLE = "Tag";

        private ItemSelectionList()
        {
            InitializeComponent();
            DataContext = this;

            ((INotifyCollectionChanged)lbox_selectionList.Items).CollectionChanged += DetermineListPrompt;
            Loaded += (s, e) =>
            {
                SelectionList = OnGetSourceList(this, EventArgs.Empty);
                FilterList();

                // Select previous items
                UpdateSelectedItems();
            };
        }

        public async static Task<Audio> SelectAudio(Audio currentAudio, Type audioType = null, List<Audio> selectionList = null)
        {
            ItemSelectionList itemSelectionList = PrepareAudioList(currentAudio is null ? null : new() { currentAudio }, audioType, selectionList, false);

            return (Audio)await ShowDialog(currentAudio, itemSelectionList);
        }

        public async static Task<List<Audio>> SelectAudios(List<Audio> currentAudios, Type audioType = null, List<Audio> selectionList = null)
        {
            ItemSelectionList itemSelectionList = PrepareAudioList(currentAudios, audioType, selectionList, true);

            return (await ShowDialog(currentAudios, itemSelectionList)).Cast<Audio>().ToList();
        }

        public async static Task<Category> SelectCategory(Category currentCategory, List<Category> selectionList = null)
        {
            ItemSelectionList itemSelectionList = PrepareCategoryList((currentCategory is null || currentCategory.IsDefault()) ? null : new() { currentCategory }, selectionList, false);

            Category newCategory = (Category)await ShowDialog(currentCategory, itemSelectionList);
            if (newCategory is null)
                return Category.Default;

            return newCategory;
        }

        public async static Task<List<Category>> SelectCategories(List<Category> currentCategories, List<Category> selectionList = null)
        {
            ItemSelectionList itemSelectionList = PrepareCategoryList(currentCategories, selectionList, true);

            return (await ShowDialog(currentCategories, itemSelectionList)).Cast<Category>().ToList();
        }

        public async static Task<Tag> SelectTag(Tag currentTag, List<Tag> selectionList = null)
        {
            ItemSelectionList itemSelectionList = PrepareTagList(currentTag is null ? null : new() { currentTag }, selectionList, false);

            return (Tag)await ShowDialog(currentTag, itemSelectionList);
        }

        public async static Task<List<Tag>> SelectTags(List<Tag> currentTags, List<Tag> selectionList = null)
        {
            ItemSelectionList itemSelectionList = PrepareTagList(currentTags, selectionList, true);

            return (await ShowDialog(currentTags, itemSelectionList)).Cast<Tag>().ToList();
        }

        #region Prepare Functions
        private static ItemSelectionList PrepareAudioList(List<Audio> currentAudios, Type audioType, List<Audio> selectionList, bool multiSelection)
        {
            ItemSelectionList itemSelectionList = PrepareList(AUDIO_TITLE, currentAudios, selectionList, multiSelection);
            itemSelectionList.OnGetSourceList += (s, e) =>
            {
                List<Audio> audioList = new();
                if (selectionList is null)
                    audioList = AppModel.Instance.AudioList.ToList();
                else
                    audioList = selectionList;

                if (audioType is not null)
                    audioList = audioList.Where(a => a.GetType() == audioType).ToList();

                return audioList;
            };

            return itemSelectionList;
        }

        private static ItemSelectionList PrepareCategoryList(List<Category> currentCategories, List<Category> selectionList, bool multiSelection)
        {
            ItemSelectionList itemSelectionList = PrepareList(CATEGORY_TITLE, currentCategories, selectionList, multiSelection);
            itemSelectionList.OnGetSourceList += (s, e) =>
            {
                if (selectionList is null)
                    return AppModel.Instance.Categories.Where(c => !c.IsDefault());

                return selectionList;
            };
            itemSelectionList.OnAddItem += (s, categoryName) => AppModel.Instance.Categories.Add(new(categoryName));
            itemSelectionList.OnRemoveItem += (s, category) => AppModel.Instance.RemoveCategory((Category)category);
            itemSelectionList.IsAddEnabled = true;
            itemSelectionList.IsRemoveEnabled = true;

            return itemSelectionList;
        }

        private static ItemSelectionList PrepareTagList(List<Tag> currentTags, List<Tag> selectionList, bool multiSelection)
        {
            ItemSelectionList itemSelectionList = PrepareList(TAG_TITLE, currentTags, selectionList, multiSelection);
            itemSelectionList.OnGetSourceList += (s, e) =>
            {
                if (selectionList is null)
                    return AppModel.Instance.Tags;

                return selectionList;
            };
            itemSelectionList.OnAddItem += (s, tagName) => AppModel.Instance.Tags.Add(new(tagName));
            itemSelectionList.OnRemoveItem += (s, tag) => AppModel.Instance.RemoveTag((Tag)tag);
            itemSelectionList.IsAddEnabled = true;
            itemSelectionList.IsRemoveEnabled = true;

            return itemSelectionList;
        }

        private static ItemSelectionList PrepareList(string title, IEnumerable<object> currentValues, IEnumerable<object> selectionList, bool multiSelection)
        {
            ItemSelectionList itemSelectionList = new();
            itemSelectionList.itemTitle = title;
            itemSelectionList.NotifyPropertyChanged(nameof(PageTitle));
            itemSelectionList.NotifyPropertyChanged(nameof(NoItemsText));
            itemSelectionList.NotifyPropertyChanged(nameof(NoItemsFoundText));
            itemSelectionList.NotifyPropertyChanged(nameof(NoItemsSelectedText));
            itemSelectionList.previousItems = currentValues;
            itemSelectionList.SelectedItems = currentValues is null? new() : currentValues.ToList();
            itemSelectionList.SelectionList = selectionList;
            itemSelectionList.MultiSelection = multiSelection;
            return itemSelectionList;
        }
        #endregion

        private async static Task<object> ShowDialog(object currentValue, ItemSelectionList itemSelectionList)
        {
            IEnumerable<object> newValues = await ShowDialog(new List<object>() { currentValue }, itemSelectionList);
            if (newValues.Count() == 0)
                return null;

            return newValues.First();
        }

        private async static Task<IEnumerable<object>> ShowDialog(IEnumerable<object> currentValues, ItemSelectionList itemSelectionList)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            List<object> newValues = await mainWindow.ShowDialog<List<object>>(itemSelectionList);
            if (newValues is null)
                return currentValues;

            return newValues;
        }

        private bool FilterItem(object obj)
        {
            if (SearchItem is null)
                return obj.ToString().ToLower().Contains(SearchText.ToLower());

            return SearchItem(obj, SearchText);
        }

        private void FilterList()
        {
            if (SelectionListCollectionView is null)
                return;

            SelectionListCollectionView.Refresh();
            UpdateSelectedItems();
        }

        private void UpdateSelectedItems()
        {
            NotifyPropertyChanged(nameof(SelectedItems));
            NotifyPropertyChanged(nameof(SelectedText));

            if (SelectionMode == SelectionMode.Single)
            {
                lbox_selectionList.SelectedItem = HelperFunctions.IsNullOrEmpty(SelectedItems) ? null : SelectedItems.First();
            }
            else
            {
                foreach (object item in SelectedItems)
                {
                    object listItem = SelectionList.FirstOrDefault(s => s.Equals(item));
                    if (listItem is not null)
                        lbox_selectionList.SelectedItems.Add(listItem);
                }
            }
        }

        private void Search_KeyUp(object sender, KeyEventArgs e)
        {
            if (!IsAddEnabled)
                return;

            if (e.Key == Key.Enter)
            {
                OnAddItem?.Invoke(this, SearchText);
                FilterList();
            }
        }

        private void DetermineListPrompt(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectionListCollectionView.IsEmpty)
            {
                if (SelectionList.Any())
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
            FilterList();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            object item = ((Control)sender).DataContext;

            if (SelectedItems.Contains(item))
                SelectedItems.Remove(item);

            OnRemoveItem?.Invoke(this, item);
            FilterList();
        }

        private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;

            if (SelectionMode == SelectionMode.Single)
            {
                if (SelectedItems.Contains(listBoxItem.DataContext))
                {
                    SelectedItems.Clear();
                    NotifyPropertyChanged(nameof(SelectedText));
                    lbox_selectionList.UnselectAll();
                }
                else
                {
                    SelectedItems.Clear();
                    SelectedItems.Add(listBoxItem.DataContext);
                    NotifyPropertyChanged(nameof(SelectedText));
                }

                return;
            }

            if (listBoxItem.IsSelected)
            {
                SelectedItems.Remove(listBoxItem.DataContext);
                NotifyPropertyChanged(nameof(SelectedText));

                return;
            }

            SelectedItems.Add(listBoxItem.DataContext);
            NotifyPropertyChanged(nameof(SelectedText));
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Deactivated when multiSelection is enabled
            if (multiSelection)
                return;

            Close(new List<object> { ((ListBoxItem)sender).DataContext });
        }

        #region Category
        private void PickNewColor_Click(object sender, RoutedEventArgs e)
        {
            popup_colorPicker.IsOpen = true;
            categoryToSelectColor = ((sender as Button).Tag as Category);
            colpck_popUpColorPicker.PickNewColor(categoryToSelectColor.Color);
        }

        private void popup_colorPicker_Closed(object sender, EventArgs e)
        {
            if (categoryToSelectColor == null)
                return;

            categoryToSelectColor.Color = colpck_popUpColorPicker.NewColor;
            AppModel.Instance.ChangeCategoryColor(categoryToSelectColor, colpck_popUpColorPicker.NewColor);
            categoryToSelectColor = null;
        }
        #endregion

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close(SelectedItems);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
