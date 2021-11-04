//<!--  -Copyright 2021 Alexandra Dorey / Wellington Fabrics  -->﻿﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WFInventory.Cloud;
using FSCommon;

namespace WFInventory.ViewModels
{
    [DocumentVM]
    public class DocumentViewModel : iDocumentViewModel
    {
        private string startText = "";
        private string endText = "";
        private string highlightdText = "";
        private Brush highLightBrush = Brushes.Yellow;
        private string actualss = "";
        private string targetLower = "";
        private string currentSearchProperty = "";
        private iDocumentViewModel _parent = null;

        public String Id { get => document.Id; }

        protected void DeleteNodeExecute(object obj)
        {
            DeleteNode(null);

        }
        virtual protected bool CanExecuteDelete(object obj)
        {
            return DeleteAllowed;
        }
        protected string _name;


        protected bool CanExecute(object obj)
        {
            return true;
        }

        protected bool CanNotExecute(object obj)
        {
            return false;
        }


        virtual protected bool DeleteAllowed
        {
            get
            {
                int t = 0;
                foreach (var tg in TypeGroups.Values)
                {
                    t += tg.Children.Count;
                }
                if (t == 0) return true;
                return false;
            }
        }
        protected virtual void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
         public Dictionary<Type, DocumentViewModelGroup> TypeGroups = new Dictionary<Type, DocumentViewModelGroup>();
        //Temporary until I regenerate all the VM Classes...

        public DocumentViewModel(FirestoreNode doc)
        {
            document = doc;
            GroupName = document.GetType().Name;
            document.PropertyChanged += FSNodePropertyChanged;
            //document.PropertyChanged += FSNodePropertyChanged;
            SearchName = doc.Id;
            //_propertiesView = new propertiesViewModel(this, document);
            //ShowProperties = true;
            DeleteNodeCommand = new relayCommand(CanExecuteDelete, DeleteNodeExecute);
            StartText = document.Name;
            PropertyChanged += VMPropertyChanged;
        }

        public virtual void VMPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<iDocumentViewModel> Children { get; set; } = new ObservableCollection<iDocumentViewModel>();
        public string GroupName { get; set; }

        public string StartText
        {
            get => startText; set
            {
                if (startText != value)
                {
                    startText = value;
                    onPropertyChanged("StartText");
                }
            }
        }

        public string EndText
        {
            get => endText;
            set
            {
                if (endText != value)
                {
                    endText = value;
                    onPropertyChanged("EndText");
                }
            }
        }

        public string HighlightedText
        {
            get => highlightdText; set
            {
                if (highlightdText != value)
                {
                    highlightdText = value;
                    onPropertyChanged("HighlightedText");
                }
            }
        }

        public Brush HighlightBrushBG { get; set; } = Brushes.Yellow;
        public Brush HighlightBrushFG { get; set; } = Brushes.Black;
        public bool SearchIgnoresCase { get; set; } = true;
        public FirestoreNode document { get; set; }
        public virtual string SearchName
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    StartText = SearchName;
                    EndText = "";
                    HighlightedText = "";
                    onPropertyChanged("SearchName");
                }
            }
        }

        private string _message = "Ready";
        public ObservableCollection<KeyValuePair<DateTime, string>> messageList { get; } = new ObservableCollection<KeyValuePair<DateTime, string>>();
        public virtual string StatusBarMessage
        {
            get => _message;
            set
            {
                messageList.Add(new KeyValuePair<DateTime, string>(DateTime.UtcNow, value));
                _message = DateTime.UtcNow.ToLocalTime().ToString() + " - " + value;
                onPropertyChanged("StatusBarMessage");
            }
        }

        public bool isRoot { get; set; } = false;
        public ICommand DeleteNodeCommand
        {
            get;
            internal set;
        }
        public virtual iDocumentViewModel Parent
        {
            get
            {
                if (_parent == null && !isRoot)
                {
                    Parent = FS.GetVM(document.Parent);
                    if (_parent != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => (_parent as DocumentViewModel).LinkChild(this)));
                    }
                }
                return _parent;
            }
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    onPropertyChanged("Parent");
                }
            }
        }
        public string this[string name]
        {
            get
            {
                Type t = GetType();
                PropertyInfo pinfo = t.GetProperty(name);
                return pinfo.GetValue(this).ToString();
            }
        }
        static public bool commandEnabled(object para)
        {
            return true;
        }

        static public bool commandDisabled(object para)
        {
            return false;
        }
        public virtual bool SecondarySearch(string pss, string sss)
        {
            if(DoSearch(pss))
            {
                return DoSearch(sss);
            }
            return false;
        }

        public virtual void FSNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            onPropertyChanged(e.PropertyName);
            if(e.PropertyName == currentSearchProperty)
            {
                onPropertyChanged("HighlightedText");
                onPropertyChanged("EndText");
                onPropertyChanged("StartText");
            }

        }
        //public virtual int CompareTo(iDocumentViewModel other)
        //{
        //    return document.CompareTo(other.document);
        //}

        public bool DoSearch(string ss)
        {
            if (SearchIgnoresCase)
            {
                actualss = ss.ToLower();
                targetLower = _name.ToLower();
            }
            else
            {
                actualss = ss;
                targetLower = _name;
            }

            if (targetLower.Contains(actualss))
            {
                int MatchedStart = targetLower.IndexOf(actualss);
                string Start = _name.Substring(0, MatchedStart);
                string mid = _name.Substring(MatchedStart, ss.Length);
                string End = _name.Substring(MatchedStart + ss.Length);
                StartText = Start;
                HighlightedText = mid;
                EndText = End;
                return true;
            }

            StartText = _name;
            HighlightedText = "";
            EndText = "";
            return false;
        }
        public void LinkChild(iDocumentViewModel child)
        {
            Type childtype = child.GetType();

            if (!TypeGroups.ContainsKey(childtype))
            {
                TypeGroups.Add(child.GetType(), new DocumentViewModelGroup(childtype.Name));
                Children.Add(TypeGroups[childtype]);
            }

            TypeGroups[childtype].LinkChild(child);
        }

        public void UnLinkChild(iDocumentViewModel child)
        {
            Type childtype = child.GetType();
            TypeGroups[childtype].UnLinkChild(child);
        }

        public ObservableCollection<iDocumentViewModel> GetTypeGroup(Type type)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (!TypeGroups.ContainsKey(type))
                {
                    TypeGroups.Add(type, new DocumentViewModelGroup(type.Name));
                    Children.Add(TypeGroups[type]);
                }
            }));
            return TypeGroups[type].Children;
        }
        
        virtual public async void DeleteNode(object para)
        {
            (Parent as DocumentViewModel).UnLinkChild(this);
            await document.Delete();
        }
    }
}
