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
                if (value is null)
                    selectedItems = new List<object>();
                else
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
                FilterList();

                // Select previous items
                if (SelectionMode == SelectionMode.Single)
                {
                    lbox_selectionList.SelectedItem = HelperFunctions.IsNullOrEmpty(SelectedItems) ? null : SelectedItems.First();
                }
                else
                {
                    foreach (object item in SelectedItems)
                    {
                        lbox_selectionList.SelectedItems.Add(item);
                    }
                }
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

            return (Category)await ShowDialog(currentCategory, itemSelectionList);
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
            itemSelectionList.OnRemoveItem += (s, tag) => AppModel.Instance.Tags.Remove((Tag)tag);
            itemSelectionList.IsAddEnabled = true;
            itemSelectionList.IsRemoveEnabled = true;

            return itemSelectionList;
        }

        private static ItemSelectionList PrepareList(string title, IEnumerable<object> currentValues, IEnumerable<object> selectionList, bool multiSelection)
        {
            ItemSelectionList itemSelectionList = new();
            itemSelectionList.itemTitle = title;
            itemSelectionList.SelectedItems = currentValues;
            itemSelectionList.SelectionList = selectionList;
            itemSelectionList.MultiSelection = multiSelection;
            return itemSelectionList;
        }
        #endregion

        private async static Task<object> ShowDialog(object currentValue, ItemSelectionList itemSelectionList)
        {
            IEnumerable<object> newValues = await ShowDialog(new List<object>() { currentValue }, itemSelectionList);

            if (!HelperFunctions.IsNullOrEmpty(newValues))
                return newValues.First();

            return currentValue;
        }

        private async static Task<IEnumerable<object>> ShowDialog(IEnumerable<object> currentValues, ItemSelectionList itemSelectionList)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            List<object> newValues = (await mainWindow.ShowDialog<List<object>>(itemSelectionList));

            if (!HelperFunctions.IsNullOrEmpty(newValues))
                return newValues;

            return currentValues;
        }

        private void FilterList()
        {
            SelectionList = OnGetSourceList(this, EventArgs.Empty);
            FilteredSelectionList = SelectionList.Where(x => x.ToString().ToLower().Contains(SearchText.ToLower()));
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
            FilterList();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveItem?.Invoke(this, ((Control)sender).DataContext);
            FilterList();
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
