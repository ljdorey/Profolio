//Copyright 2021 Alexandra Dorey / Wellington Fabrics Ltd.

using Google.Cloud.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FSCommon {

public class ListConverterDateTime : IFirestoreConverter<AwareList<DateTime>>
{
    public AwareList<DateTime> FromFirestore(object value)
    {
        AwareList<DateTime> alist = new AwareList<DateTime>();
        foreach(Timestamp item in (value as IEnumerable))
        { 
                    alist.Add(item.ToDateTime());
        }
        return alist;
    }

    public object ToFirestore(AwareList<DateTime> value)
    {
        List<Timestamp> newlist = new List<Timestamp>();
        foreach (DateTime dt in value.baselist)
            newlist.Add(Timestamp.FromDateTime(dt));
        return newlist;
    }
}

public class ListConverter<T>: IFirestoreConverter<AwareList<T>>
{
    public AwareList<T> FromFirestore(object value)
    {
        AwareList<T> alist = new AwareList<T>();
        Type t = typeof(T);
        if (t.IsEnum)
        {
            foreach (Int64 item in (value as IEnumerable))
            {
                alist.Add((T)Enum.Parse(t, item.ToString()));
            }
        }
        else
        {
            foreach (T item in (value as IEnumerable))
            {
                alist.Add(item);
            }
        }
        return alist;
    }

    public object ToFirestore(AwareList<T> value)
    {
        Type t = typeof(T);
        if (t.IsEnum)
        {
            List<Int64> newlist = new List<Int64>();
            foreach(T item in value.baselist)
            {
                newlist.Add((long)(int)Enum.Parse(t, item.ToString()));
            }
            return newlist;
        }
        else return value.baselist;
    }
}


public class AwareList<T> : IList<T>
{
    public string PropertyName { get; set; } = "";
    public FirestoreNode Parent { get; set; } = null;

    public List<T> baselist;

    public AwareList()
    {
        baselist = new List<T>();
    }

    private void forceCloudUpdate()
    {
        if (Parent != null)
        {
            Parent.UpdateProperty(PropertyName);
        }
    }

    public AwareList(IList<T> sourcelist, FirestoreNode parent)
    {
        baselist = new List<T>();
        PropertyName = (sourcelist as AwareList<T>).PropertyName;
        foreach(T item in sourcelist)
        {
            baselist.Add(item);
        }
        Parent = parent;
        
    }

    
    public T this[int index] { get => ((IList<T>)baselist)[index]; set { ((IList<T>)baselist)[index] = value;
            forceCloudUpdate();
        } }

    public int Count => ((ICollection<T>)baselist).Count;

    public bool IsReadOnly => ((ICollection<T>)baselist).IsReadOnly;

    public void Add(T item)
    {
        ((ICollection<T>)baselist).Add(item);
        forceCloudUpdate();
    }

    public void Clear()
    {
        ((ICollection<T>)baselist).Clear();
        forceCloudUpdate();

    }
    public bool Contains(T item)
    {
        return ((ICollection<T>)baselist).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)baselist).CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)baselist).GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return ((IList<T>)baselist).IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        ((IList<T>)baselist).Insert(index, item);
        forceCloudUpdate();
    }

    public bool Remove(T item)
    {
        bool retval = ((ICollection<T>)baselist).Remove(item);
        forceCloudUpdate();
        return retval;
    } 

    public void RemoveAt(int index)
    {
        ((IList<T>)baselist).RemoveAt(index);
        forceCloudUpdate();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)baselist).GetEnumerator();
    }

        public void AddRange(IEnumerable<T> list)
        {
            foreach (T item in list) Add(item);
        }
    }
}
