using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FSCommon;

namespace WFInventory.ViewModels
{
    
    public class FilterChipWrapper : INotifyPropertyChanged
    {
        public iDocumentViewModel Document { get; private set; }
        private bool _ischecked;
        public string Name { get => Document.SearchName; }

        public bool IsChecked { get => _ischecked; 
            set { 
            if(value != _ischecked)
                {
                    _ischecked = value;
                    onPropertyChanged("IsChecked");
                }
            } }
        public FilterChipWrapper(iDocumentViewModel doc, bool ischecked)
        {
            _ischecked = ischecked;
            Document = doc;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    

    public class ObservableFilterChipList : INotifyCollectionChanged, IEnumerable
    {
        ObservableCollection<FilterChipWrapper> list = new ObservableCollection<FilterChipWrapper>();
        Dictionary<iDocumentViewModel, FilterChipWrapper> index = new Dictionary<iDocumentViewModel, FilterChipWrapper>();

        AwareList<string> _ids;
        
        ICollectionView cv;
        CollectionViewSource checkedcv;

        public ICollectionView CheckedList { get => checkedcv.View; }

        public string Title { get; set; }


        private void EditCommand(object o)
        {

        }

        public ICommand EditListCommand
        {
            get;
            internal set;
        }


        static public bool commandEnabled(object para)
        {
            return true;
        }
        public ObservableFilterChipList(ObservableCollection<iDocumentViewModel> srclist, AwareList<string> ids, string title = "")
        {
            if (title == "")
                title = ids.PropertyName;

            Title = title;
            _ids = ids;
            srclist.CollectionChanged += Srclist_CollectionChanged;
            ids.Parent.PropertyChanged += Document_PropertyChanged;
            
            foreach(var i in srclist)
            {
                list.Add(new FilterChipWrapper(i, _ids.Contains(i.document.Id)));
                index.Add(i, list.Last());
                list.Last().PropertyChanged += ObservableFilterChipList_PropertyChanged;
            }

            cv = CollectionViewSource.GetDefaultView(this);
            using (cv.DeferRefresh())
            {
                cv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                cv.Filter = SearchFilter;
            }

            checkedcv = new CollectionViewSource();
            checkedcv.Source = this;
            using (checkedcv.DeferRefresh())
            {
                checkedcv.View.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                checkedcv.View.Filter = CheckedOnlyFilter;
            }

            EditListCommand = new relayCommand(commandEnabled, EditList);
        }


        public void EditList(object para)
        {
            MultiItemSelector ds = new MultiItemSelector(this);
            bool res = ds.ShowDialog() ?? false;
        }

        public void Search(string searchText, string secondarySearchText)
        {
            _searchText = searchText;
            _secondarySearchText = secondarySearchText;
            cv.Refresh();
        }

        private string _searchText ="";
        private string _secondarySearchText = "";

        private bool SearchFilter(object input)
        {
            return (input as FilterChipWrapper).Document.SecondarySearch(_searchText, _secondarySearchText);
        }

        private bool CheckedOnlyFilter(object input)
        {
            return (input as FilterChipWrapper).IsChecked;
        }


        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == _ids.PropertyName)
            {
                Type t = _ids.Parent.GetType();
                var pinfo = t.GetProperty(_ids.PropertyName);

                _ids = (AwareList<string>)pinfo.GetValue(_ids.Parent);
                UpdateList(_ids);
            }
        }

        private void UpdateList(IList<string> newlist)
        {
            foreach(var fcw in list)
            {
                if(_ids.Contains(fcw.Document.document.Id))
                {
                    if (!fcw.IsChecked)
                    {
                        fcw.IsChecked = true;
                        
                    }
                }
                else
                {
                    if (fcw.IsChecked)
                    {
                        fcw.IsChecked = false;
                    }
                }
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                cv.Refresh();
                checkedcv.View.Refresh();
            }));
        }

        private void ObservableFilterChipList_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool state = (sender as FilterChipWrapper).IsChecked;
            iDocumentViewModel doc = (sender as FilterChipWrapper).Document;
            
            if (_ids.Contains(doc.document.Id))
            {
                if(!state)
                {
                    _ids.Remove(doc.document.Id);
                }
            }
            else
            {
                if(state)
                {
                    _ids.Add(doc.document.Id);
                }
            }
            checkedcv.View.Refresh();
        }

        private void Srclist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(iDocumentViewModel i in e.NewItems)
                    {
                        list.Add(new FilterChipWrapper(i, _ids.Contains(i.document.Id)));
                        index.Add(i, list.Last());
                        list.Last().PropertyChanged += ObservableFilterChipList_PropertyChanged;
                        if(_ids.Contains(i.Id))
                        {
                            checkedcv.View.Refresh();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (iDocumentViewModel i in e.OldItems)
                    {
                        index[i].PropertyChanged -= ObservableFilterChipList_PropertyChanged;
                        list.Remove(index[i]);
                        index.Remove(i);
                        
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    list.Clear();
                    index.Clear();
                    break;
            }

        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                ((INotifyCollectionChanged)this.list).CollectionChanged += value;
            }

            remove
            {
                ((INotifyCollectionChanged)this.list).CollectionChanged -= value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)this.list).GetEnumerator();
        }

    }



}
