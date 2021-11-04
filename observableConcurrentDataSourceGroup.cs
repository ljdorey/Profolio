using Google.Cloud.Firestore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading;
using System.Windows;

using WFInventory.ViewModels;
using static WFInventory.Util;
using DocumentChange = Google.Cloud.Firestore.DocumentChange;
using static WFInventory.ViewModels.ViewModelCommon;
using System.Threading.Tasks;
using FSCommon;
using static FSCommon.FireStoreAccess;

namespace WFInventory.Cloud
{
    //Property Notify???
    public class observableConcurrentDataSourceGroup<CT> :  INotifyCollectionChanged, IEnumerable where CT : FirestoreNode
    {
        public readonly KeyedObservableCollection<CT> nodes;
        public ConcurrentQueue<DocumentChange> ChangesFromListener = new ConcurrentQueue<DocumentChange>();

        private readonly ObservableCollection<string> SortedKeys = new ObservableCollection<string>();
        public readonly Dictionary<FirestoreNode, DocumentViewModel> NodeToVMMap = new Dictionary<FirestoreNode, DocumentViewModel>();
        
        public ObservableCollection<DocumentViewModel> ViewModels = new ObservableCollection<DocumentViewModel>();
       
        private CancellationTokenSource cts = new CancellationTokenSource();
        ~observableConcurrentDataSourceGroup()
        {
            cts.Cancel();
        }
        int changesprocessed = 0;
        DateTime startTime;

        private void queueChange(DocumentChange change)
        {
            ChangesFromListener.Enqueue(change);
        }

        public void StopListening()
        {
            cts.Cancel();
        }

        bool AllParentsFound = false;

        int times = 0;
        
        public void processQueue()
        {
            if(changesprocessed == 0 && DateTime.Now > startTime.AddSeconds(120) && !cts.IsCancellationRequested)
            {
                w($"Listener: {this.TypeName} has received no inital data after 3min, cancelling listen stream.", MessageType.CriticalError);
                StopListening();
            }
            try
            {
                if (!AllParentsFound)
                {
                    AllParentsFound = true;
                    times++;
                    foreach (FirestoreNode fsn in nodes)
                    {
                        if (fsn.Parent == null)
                        {
                            AllParentsFound = false;
                        }
                    }

                    foreach (DocumentViewModel bdvm in ViewModels)
                    {
                        if(bdvm.Parent == null)
                        {
                            AllParentsFound = false;
                        }
                    }

                }

                List<DocumentChange> UnprocessedChanges = new List<DocumentChange>();
                DocumentChange change;
                while (ChangesFromListener.TryDequeue(out change))
                {
                    try
                    {
                        DocumentSnapshot document = change.Document;
                        if (document.Exists)
                        {
                            string doccolname = change.Document.Reference.Parent.Id;
                            Type t2 = typeof(wf_Product);
                            string namepath = t2.FullName.Substring(0, t2.FullName.Length - 10);
                            Assembly dcAssembly = Assembly.GetAssembly(typeof(wf_Product));
                            string name = namepath + doccolname.Substring(0, doccolname.Length - 1);
                            Type doctype = dcAssembly.GetType(name);

                            switch (change.ChangeType)
                            {
                                case DocumentChange.Type.Added:
                                    {
                                        Type t3 = typeof(DocumentSnapshot);
                                        MethodInfo method = t3.GetMethod("ConvertTo", BindingFlags.Instance | BindingFlags.Public);
                                        MethodInfo generic = method.MakeGenericMethod(doctype);
                                        CT doc;
                                        try
                                        {
                                            doc = (CT)generic.Invoke(change.Document, null);
                                        }
                                        catch (Exception e)
                                        {
                                            w("ProcessQueue", e);
                                            throw new Exception("Buggered import...");
                                        }

                                        if (change.Document.Reference.Parent.Parent == null)
                                        {
                                            DocumentViewModel docvm = (DocumentViewModel)documentVMFactory(doc);
                                            NodeToVMMap.Add(doc, docvm);
                                             Application.Current.Dispatcher.BeginInvoke(new Action(() => ViewModels.Add(docvm)));
                                            doc.init(FS.Root);
                                            SortedKeys.Insert((int)change.NewIndex, doc.Id);
                                            nodes.Add(doc);
                                            docvm.Parent = FS.RootVM;
                                            Application.Current.Dispatcher.BeginInvoke(new Action(() => FS.RootVM.LinkChild(docvm)));
                                        }
                                        else
                                        {
                                            string parentid = change.Document.Reference.Parent.Parent.Id;
                                            string parentcolname = change.Document.Reference.Parent.Parent.Parent.Id;
                                            string parentname = namepath + parentcolname.Substring(0, parentcolname.Length - 1);
                                            Type parenttype = dcAssembly.GetType(parentname);
                                            
                                            DocumentViewModel docvm = (DocumentViewModel)documentVMFactory(doc);
                                            NodeToVMMap.Add(doc, docvm);
                                            Application.Current.Dispatcher.BeginInvoke(new Action(() => ViewModels.Add(docvm)));

                                            FirestoreNode Parent = GetNodeEx(parenttype, parentid);
                                            if (Parent == null)
                                            {
                                                AllParentsFound = false;
                                                doc.init(parenttype, parentid);
                                            }
                                            else
                                            {
                                                doc.init(Parent);
                                                DocumentViewModel parentVM = (DocumentViewModel)FS.GetVM(Parent);
                                                docvm.Parent = parentVM;
                                                Application.Current.Dispatcher.BeginInvoke(new Action(() => parentVM.LinkChild(docvm)));
                                            }

                                            SortedKeys.Insert((int)change.NewIndex, doc.Id);
                                            nodes.Add(doc);
                                        }
                                    }
                                    break;

                                case DocumentChange.Type.Removed:
                                    {
                                        //TODO link with newTreeview Browser Unlink method..
                                        ViewModels.Remove(NodeToVMMap[nodes[change.Document.Id]]);
                                        NodeToVMMap.Remove(nodes[change.Document.Id]);
                                        nodes.Remove(change.Document.Id);
                                        SortedKeys.RemoveAt((int)change.OldIndex);
                                        if (change.Document.Reference.Parent.Parent == null)
                                        {
                                            FS.Root.DeRegisterChildEx(doctype, change.Document.Id);
                                        }
                                        else
                                        {
                                            if (change.Document.Reference.Parent != null && change.Document.Reference.Parent.Parent != null)
                                            {
                                                string parentid = change.Document.Reference.Parent.Parent.Id;
                                                string parentcolname = change.Document.Reference.Parent.Parent.Parent.Id;
                                                string parentname = namepath + parentcolname.Substring(0, parentcolname.Length - 1);
                                                Type parenttype = dcAssembly.GetType(parentname);
                                                FirestoreNode parent = GetNodeEx(parenttype, parentid);
                                                if(parent != null)
                                                parent.DeRegisterChildEx(doctype, change.Document.Id);
                                            }
                                        }
                                    }
                                    break;

                                case DocumentChange.Type.Modified:
                                    {
                                        Type t3 = typeof(DocumentSnapshot);
                                        MethodInfo method = t3.GetMethod("ConvertTo", BindingFlags.Instance | BindingFlags.Public);
                                        MethodInfo generic = method.MakeGenericMethod(doctype);
                                        FirestoreNode doc = (FirestoreNode)generic.Invoke(change.Document, null);
                                        nodes[doc.Id].processCloudUpdate(doc);
                                        SortedKeys.Move(change.OldIndex.Value, change.NewIndex.Value);
                                        ChangeFromCloud.Enqueue(nodes[doc.Id]);
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                       w("ProcessQueue", e);
                    }
                }        
            }
            catch (Exception e)
            {
               w("ProcessQueue", e);
            }
        }

        FirestoreChangeListener Listener;
        public observableConcurrentDataSourceGroup(Type t) : base()
        {
            nodes = new KeyedObservableCollection<CT>();
            var token = cts.Token;
            TypeName = t.Name;

            Listener = CloudCommon.FireStore.CollectionGroup(t.Name + "s").Listen(snapShot =>
            {
                foreach (DocumentChange change in snapShot.Changes)
                {
                    queueChange(change);
                    changesprocessed++;
                }
                // w(Listener.ListenerTask.Status.ToString());
            }, token);

            startTime = DateTime.Now;

            SortedKeys.CollectionChanged += collectionChangePassthrough;
            StdOut.WriteLine("DataSource", $"Watching: {t.Name}");
            
        }

        public string TypeName { get; private set; }

        private void collectionChangePassthrough(object caller, NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }


        
        public int Count => SortedKeys.Count;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public CT this[string key]
        {
            get
            {
                if (nodes.ContainsKey(key)) return (CT)nodes[key];
                return null;
            }
        }

        public CT this[int key] => (CT)nodes[SortedKeys[key]];

        public IEnumerator GetEnumerator()
        {
            return new observableConcurrentDataSourceGroupEnum<CT>(this);
        }
    }


    public class observableConcurrentDataSourceGroupEnum<CT> : IEnumerator where CT : FirestoreNode
    {
        private readonly observableConcurrentDataSourceGroup<CT> _dataSource;

        private int _position = -1;

        public observableConcurrentDataSourceGroupEnum(observableConcurrentDataSourceGroup<CT> dataSource)
        {
            _dataSource = dataSource;
        }

        public object Current => _dataSource[_position];

        public bool MoveNext()
        {
            _position++;
            return (_position < _dataSource.Count);
        }

        public void Reset()
        {
            _position = -1;
        }
    }

}
