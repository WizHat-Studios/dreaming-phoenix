using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.AudioProperties;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.UserControls
{
    /// <summary>
    /// Interaction logic for AudioCategorySelection.xaml
    /// </summary>
    public partial class AudioCategorySelection : DialogControl, INotifyPropertyChanged
    {
        public AppModel AppModelInstance { get; set; } = AppModel.Instance;

        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Category> filteredCategories;
        public ObservableCollection<Category> FilteredCategories
        {
            get { return filteredCategories; }
            set
            {
                filteredCategories = value;
                NotifyPropertyChanged();
            }
        }

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
                //FilteredCategories = new(Categories.Where(x => !x.Equals(Category.Default) && x.Name.ToLower().Contains(SearchText.ToLower())));
            }
        }

        public bool SearchActive { get { return !string.IsNullOrEmpty(SearchText); } }

        public AudioCategorySelection(Category previousCategory = null)
        {
            InitializeComponent();
            DataContext = this;

            SelectedCategory = previousCategory.Equals(Category.Default) ? null : previousCategory;
            //Categories = new(AppModelInstance.Categories.Where(x => !x.Equals(Category.Default)));
            FilterList();
            SearchText = "";
            ((INotifyCollectionChanged)lbox_selectableCategoryList.Items).CollectionChanged += DetermineCategoryListPrompt;
            AppModel.Instance.Categories.CollectionChanged += (s, e) => FilterList();
        }

        private void FilterList()
        {
            Categories = new(AppModel.Instance.Categories.Where(x => !x.Equals(Category.Default)));
            FilteredCategories = new(Categories.Where(x => !x.Equals(Category.Default) && x.Name.ToLower().Contains(SearchText.ToLower())));
        }

        private void Search_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Category exactCategory = FilteredCategories.FirstOrDefault(c => c.Name.ToLower() == SearchText.ToLower());
                if (exactCategory == null)
                {
                    exactCategory = new() { Name = SearchText };
                    AppModel.Instance.Categories.Add(exactCategory);
                }

                SelectedCategory = exactCategory;
            }
        }

        private void DetermineCategoryListPrompt(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (FilteredCategories.Count == 0)
            {
                if (Categories.Count == 0)
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

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.Categories.Add(new() { Name = SearchText });
        }

        private void RemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            AppModel.Instance.RemoveCategory((Category)((Button)sender).DataContext);
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close(SelectedCategory);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CategoryListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Close(SelectedCategory);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
