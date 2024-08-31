using System.Collections.Generic;

/// <summary>
/// A very rough priority queue for simple usages.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T> where T : notnull
{
    // Public
    public IComparer<T> Comparer => comparer;
    public int Count => list.Count;
    public T this[int index]
    {
        get => list[index];
        set
        {
            list[index] = value;
            list.Sort(comparer);
        }
    }

    // Private
    private readonly List<T> list;
    private readonly IComparer<T> comparer;

    // Defined Function
    public PriorityQueue()
    {
        list = new();
        comparer = Comparer<T>.Default;
    }
    public PriorityQueue(IComparer<T> comparer) : this()
    {
        if (comparer != null)
        {
            this.comparer = comparer;
        }
    }
    public PriorityQueue(List<T> values)
    {
        list = new(values);
        comparer = Comparer<T>.Default;
        list.Sort(comparer);
    }
    public PriorityQueue(List<T> values, IComparer<T> comparer) : this(values)
    {
        if (comparer != null)
        {
            this.comparer = comparer;
            list.Sort(comparer);
        }
    }

    public void Add(T item)
    {
        int i = list.BinarySearch(0, Count, item, comparer);
        if (i < 0) i = ~i;
        list.Insert(i, item);
    }

    public void Clear()
    {
        list.Clear();
    }

    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    public bool Remove(T item)
    {
        return list.Remove(item);
    }

    public int IndexOf(T item)
    {
        int ret = list.BinarySearch(0, Count, item, comparer);
        return ret >= 0 ? ret : -1;
    }
}
