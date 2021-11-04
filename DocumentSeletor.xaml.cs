using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using WFInventory.ViewModels;

namespace WFInventory
{
    /// <summary>
    /// Interaction logic for DocumentSeletor.xaml
    /// </summary>
    public partial class DocumentSelector : Window, INotifyPropertyChanged
    {
        ObservableCollection<iDocumentViewModel> docs = new ObservableCollection<iDocumentViewModel>();
        public ObservableCollection<iDocumentViewModel> ItemsToSearch { get => docs; set { docs = value; onPropertyChanged("ItemsToSearch"); } }

        public DocumentSelector(ObservableCollection<iDocumentViewModel> itemsToSearch)
        {
            InitializeComponent();
            DataContext = this;
            cv = CollectionViewSource.GetDefaultView(itemsToSearch);
            using (cv.DeferRefresh())
            {
                    orifilter = cv.Filter;   
                cv.Filter = SearchFilter;
                cv.SortDescriptions.Add(new SortDescription("SearchName", ListSortDirection.Ascending));
                
            }
            ItemsToSearch = itemsToSearch;
        }

        protected virtual void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ICollectionView cv;
        private void DocumentListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            itemSelected();
        }

        Predicate<object> orifilter = null;

        private void SearchField_Loaded(object sender, RoutedEventArgs e)
        {
            SearchField.Focus();
        }

        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchField.Text.Contains(' '))
            {
                string[] split = SearchField.Text.Split(' ');
                _searchText = split[0];
                _secondarySearchText= split[1];
            }
            else
            {
                _searchText = SearchField.Text;
                _secondarySearchText = "";
            }
            
            cv.Refresh();
            if (DocumentListBox.Items.Count > 0)
            {
                DocumentListBox.SelectedItem = DocumentListBox.Items[0];
            }
        }

        public DocumentViewModel DocumentSelected { get; private set; }
        public DocumentViewModel SecondarySelection { get; private set; }

        private bool SearchFilter(object input)
        {
            if (orifilter != null)
            {
                if(orifilter(input))return (input as DocumentViewModel).SecondarySearch(_searchText, _secondarySearchText);
            }
            else
            {
                return (input as DocumentViewModel).SecondarySearch(_searchText, _secondarySearchText);
            }
            return false;
        }

        private string _searchText = "";
        private string _secondarySearchText = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void SearchField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (DocumentListBox.SelectedIndex + 1 < DocumentListBox.Items.Count)
                    DocumentListBox.SelectedIndex++;
            }


            if (e.Key == Key.Up)
            {
                if (DocumentListBox.SelectedIndex > 0)
                    DocumentListBox.SelectedIndex--;
            }

            if (e.Key == Key.Enter)
            {
                itemSelected();
            }

            if (e.Key == Key.Escape)
            {
                cancel();
            }
        }

        private void cancel()
        {
            DialogResult = false;
        }

        private void itemSelected()
        {
            if (DocumentListBox.SelectedItem != null)
            {
                DocumentSelected = (DocumentViewModel)DocumentListBox.SelectedItem;
                this.DialogResult = true;
                this.Close();
              
            }
        }

        public ObservableCollection<DocumentViewModel> dummyList { get; set; } = new ObservableCollection<DocumentViewModel>();

        private void DocumentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocumentListBox.SelectedItem != null)
            {
                dummyList.Clear();
                dummyList.Add((DocumentViewModel)DocumentListBox.SelectedItem);
            }
        }


        public void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Accept the dialog and return the dialog result
            itemSelected();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            cv.Filter = orifilter;
        }
    }
}
