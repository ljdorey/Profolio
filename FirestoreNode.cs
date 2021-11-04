using Google.Cloud.Firestore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using static FSCommon.FireStoreAccess;
using static WFInventory.Util;
using WFInventory;

namespace FSCommon
{
    public class FirestoreNode : INotifyPropertyChanged, IComparable<FirestoreNode>
    {
        public object this[string name]
        {
            get
            {
                Type t = GetType();
                PropertyInfo pinfo = t.GetProperty(name);
                return pinfo.GetValue(this);
            }

            set
            {
                Type t = GetType();
                PropertyInfo pinfo = t.GetProperty(name);
                pinfo.SetValue(this, value);
            }
        }

        public FirestoreNode()
        {
            PropertyInfo[] props = GetType().GetProperties();
            CloudUpdates = false;
        }

        public string Name { get; set; } = "";
        public bool CloudUpdates { get; set; }

        public ConcurrentQueue<string> DirtyProperties = new ConcurrentQueue<string>();

        [FirestoreDocumentId] public DocumentReference DocRef { get; set; }
        [FirestoreDocumentId] public string Id { get; set; }
        [FirestoreDocumentCreateTimestamp] public DateTime CreateTime { get; set; }
        [FirestoreDocumentUpdateTimestamp] public DateTime UpdateTime { get; set; }
        [FirestoreDocumentReadTimestamp] public DateTime Readtime { get; set; }

        public bool IsRoot { get; set; }
        public Type ParentType { get; set; }
        public string ParentId { get; private set; }

        //TODO: Add Deleted Checks through-out code and DelList Queue...
        public bool Deleted { get; set; } = false;

        //TODO: Add Created Checks through-out code and Create Queue...?

        private FirestoreNode _parent = null;

        //Add existing node to tree.

        public void init(Type parenttype, string parentid)
        {
            var fslist = getFireStoreProperties();
            foreach (var prop in fslist)
            {
                if (prop.PropertyType.IsGenericType)
                {
                    if (prop.PropertyType.GetGenericTypeDefinition() == typeof(AwareList<>))
                    {
                        var nameproperty = prop.PropertyType.GetProperty("PropertyName");
                        var parentproperty = prop.PropertyType.GetProperty("Parent");
                        object o = prop.GetValue(this);
                        parentproperty.SetValue(o, this);
                        nameproperty.SetValue(o, prop.Name);
                    }
                }
            }
            ParentType = parenttype;
            ParentId = parentid;
            CloudUpdates = true;
            LocalUpdates = true;
            Name = this.GetType().Name + Id;
        }

        public bool LocalUpdates { get; set; } = false;

        public void init(FirestoreNode parent)
        {
            var fslist = getFireStoreProperties();
            foreach (var prop in fslist)
            {
                if (prop.PropertyType.IsGenericType)
                {
                    if (prop.PropertyType.GetGenericTypeDefinition() == typeof(AwareList<>))
                    {
                        var nameproperty = prop.PropertyType.GetProperty("PropertyName");
                        var parentproperty = prop.PropertyType.GetProperty("Parent");
                        object o = prop.GetValue(this);
                        parentproperty.SetValue(o, this);
                        nameproperty.SetValue(o, prop.Name);
                    }
                }
            }

            CloudUpdates = true;
            LocalUpdates = true;
            if (parent == null)
            {
                IsRoot = true;
                Name = "Root";
            }
            else
            {
                Parent = parent;
                Parent.RegisterChildEx(this);
                Name = this.GetType().Name + Id;
            }
        }

        public void RegisterChild<T>(T child) where T : FirestoreNode
        {
            GetChildren<T>().Add(child);
            if (GetNodes<T>().ContainsKey(child.Id))
            {
                GetNodes<T>().Remove(child.Id);
            }
            GetNodes<T>().Add(child);
        }

        public void DeRegisterChild<T>(T child) where T : FirestoreNode
        {
            DeRegisterChildById<T>(child.Id);
        }

        public void DeRegisterChildById<T>(string id) where T : FirestoreNode
        {
            GetChildren<T>().Remove(id);
            if (GetNodes<T>().ContainsKey(id))
            {
                GetNodes<T>().Remove(id);
            }
        }

        public void RegisterChildEx(FirestoreNode node)
        {
            Type t = node.GetType();
            var mi = t.GetMethod("RegisterChild", BindingFlags.Instance | BindingFlags.Public);
            var mi2 = mi.MakeGenericMethod(node.GetType());
            object[] parmlist = new object[1];
            parmlist[0] = node;
            mi2.Invoke(this, parmlist);
        }

        public void DeRegisterChildEx(Type t, string id)
        {
            var mi = t.GetMethod("DeRegisterChildById", BindingFlags.Instance | BindingFlags.Public);
            var mi2 = mi.MakeGenericMethod(t);
            object[] parmlist = new object[1];
            parmlist[0] = id;
            mi2.Invoke(this, parmlist);
        }

        public List<PropertyInfo> getFireStoreProperties()
        {
            List<PropertyInfo> fsprops = new List<PropertyInfo>();
            PropertyInfo[] allprops = GetType().GetProperties();
            foreach (PropertyInfo pi in allprops)
            {
                if (pi.GetCustomAttributes<FirestorePropertyAttribute>().Any())
                {
                    fsprops.Add(pi);
                }
            }
            return fsprops;
        }

        public void processCloudUpdate(FirestoreNode update)
        {
            CloudUpdates = false;
            //  foreach (var updateAction in propertyUpdateActions.Values) updateAction.Invoke(update);
            PropertyInfo[] props = update.GetType().GetProperties();
            foreach (PropertyInfo pi in props)
            {
                if (pi.GetCustomAttributes<FirestorePropertyAttribute>().Any())
                {
                    //updateToCloud[pi.Name] = false;
                    //could be to slow??
                    pi.SetValue(this, pi.GetValue(update));
                    if (pi.PropertyType.IsGenericType)
                    {
                        if (pi.PropertyType.GetGenericTypeDefinition() == typeof(AwareList<>))
                        {
                            var nameproperty = pi.PropertyType.GetProperty("PropertyName");
                            var parentproperty = pi.PropertyType.GetProperty("Parent");
                            object o = pi.GetValue(this);
                            parentproperty.SetValue(o, this);
                            nameproperty.SetValue(o, pi.Name);
                        }
                    }
                    //updateToCloud[pi.Name] = true;
                }
            }
            CloudUpdates = true;
        }

        public void UpdateProperty(string propertyName)
        {
            if (CloudUpdates)
            {
                DirtyProperties.Enqueue(propertyName);
                CloudUpdateRequired.Enqueue(this);
            }
            LastUpdateTime = DateTime.Now;
            onPropertyChanged(propertyName);
        }
        public DateTime LastUpdateTime { get; private set; }
        public FirestoreNode Parent
        {
            get
            {
                if (_parent == null && !IsRoot)
                {
                    if (ParentType != null && ParentId != "")
                    {
                        
                        _parent = GetNodeEx(ParentType, ParentId);
                        if (_parent != null)
                        {
                            _parent.RegisterChildEx(this);
                            onPropertyChanged("Parent");
                        }
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

        public async Task<string> CreateChildForceID<TC>(TC child, string id) where TC: FirestoreNode
        {
            try
            {
                Type t = typeof(TC);
                CollectionReference col;

                if (DocRef == null)
                {
                    col = FireStore.Collection(t.Name + "s");
                }
                else
                {
                    col = DocRef.Collection(t.Name + "s");
                }

                var docref = col.Document(id);
                child.DocRef = docref;
                await docref.CreateAsync(child);
                child.Id = docref.Id;
                if (InitAfterCreate) child.init(this);
                return docref.Id;
            }
            catch (Exception E)
            {
                wl("-------------------------------------------------------------------------------");
                wl(E.ToString());
                wl("-------------------------------------------------------------------------------");
            }
            return null;
        }

        public async Task<string> CreateChild<TC>(TC child) where TC : FirestoreNode
        {
            try
            {
                Type t = typeof(TC);
                CollectionReference col;

                if (DocRef == null)
                {
                    col = FireStore.Collection(t.Name + "s");
                }
                else
                {
                    col = DocRef.Collection(t.Name + "s");
                }

                DocumentReference newdoc = await col.AddAsync(child);
                child.DocRef = newdoc;
                child.Id = newdoc.Id;
                if (InitAfterCreate) child.init(this);
                return newdoc.Id;
            }
            catch (Exception E)
            {
                wl("-------------------------------------------------------------------------------");
                wl(E.ToString());
                wl("-------------------------------------------------------------------------------");
            }
            return null;
        }

        public async Task CreateChildren<TC>(List<TC> children) where TC : FirestoreNode, new()
        {
            //w("Running AddChildren...");
            try
            {
                Type t = typeof(TC);
                CollectionReference col;
                if (DocRef == null)
                {
                    col = FireStore.Collection(t.Name + "s");
                }
                else
                {
                    col = DocRef.Collection(t.Name + "s");
                }

                WriteBatch batch = FireStore.StartBatch();
                int sets = 0;
                // w($"{children.Count} items..");
                foreach (TC child in children)
                {
                    DocumentReference newdocref = col.Document();
                    child.Id = newdocref.Id;
                    child.DocRef = newdocref;
                    batch.Set(newdocref, child);
                    if (InitAfterCreate)
                    {
                        child.init(this);
                    }
                    sets++;
                    if (sets > 480)
                    {
                        //    w($"Comitting IB (1s delay)..");
                        await batch.CommitAsync();
                        await Task.Delay(1000);
                        batch = FireStore.StartBatch();
                        sets = 0;
                    }
                }
                //   w("Final Comit..");
                await batch.CommitAsync();
                //                w("Done\n");
            }
            catch (Exception E)
            {
                wl("-------------------------------------------------------------------------------");
                wl(E.ToString());
                wl("-------------------------------------------------------------------------------");
            }
        }

        public void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ForEach<CT>(Action<CT> foreachfunction) where CT : FirestoreNode
        {
            foreach (CT item in GetChildren<CT>())
            {
                foreachfunction(item);
            }
        }

        public virtual void CheckForThings(List<ThingToDo> things)
        {
            foreach (KeyedObservableCollection<FirestoreNode> nodes in _children.Values)
            {
                foreach (FirestoreNode node in nodes)
                {
                    node.CheckForThings(things);
                }
            }
        }

        public async Task Delete()
        {
            if (DocRef != null)
            {
                List<CollectionReference> collections = await DocRef.ListCollectionsAsync().ToListAsync();
                if (collections.Count > 0)
                {
                    return;
                }
                //TODO Add to delist...
                await DocRef.DeleteAsync();
                if (InitAfterCreate)
                {
                    Parent.DeRegisterChildEx(this.GetType(), Id);
                }
                Deleted = true;
            }
        }

        public string this[Type t]
        {
            get
            {
                if (!_children.ContainsKey(t))
                {
                    _children[t] = new KeyedObservableCollection<FirestoreNode>();
                }
                return "errorfinding";
            }
        }

        protected ConcurrentDictionary<Type, object> _children =
                                                            new ConcurrentDictionary<Type, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        public KeyedObservableCollection<CT> GetChildren<CT>() where CT : FirestoreNode
        {
            if (!_children.ContainsKey(typeof(CT)))
            {
                _children[typeof(CT)] = new KeyedObservableCollection<CT>();
            }
            return (KeyedObservableCollection<CT>)_children[typeof(CT)];
        }

        public T GetChild<T>(string id) where T : FirestoreNode
        {
            if(GetChildren<T>().ContainsKey(id))return GetChildren<T>()[id];
            return null;
        }

        public virtual int CompareTo(FirestoreNode other)
        {
            return other.Name.CompareTo(Name);
        }
    }
}