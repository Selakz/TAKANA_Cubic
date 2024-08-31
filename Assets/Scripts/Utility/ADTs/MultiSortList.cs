using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MultiSortList<T> : IEnumerable<T>
{
    // Serializable and Public
    public int Count => list.Count;

    public T this[string sortBy, int index]
    {
        get
        {
            if (sortIndexes[sortBy] == null) return default;
            return list[sortIndexes[sortBy][index]];
        }
    }

    public T this[int index] => list[index];

    public List<string> SortLabels => sortIndexes.Keys.ToList();

    // Private
    private readonly List<T> list;
    private readonly Dictionary<string, PriorityQueue<int>> sortIndexes;
    private readonly Dictionary<string, Comparison<T>> comparisons;

    // Defined Function
    public MultiSortList()
    {
        list = new();
        sortIndexes = new();
        comparisons = new();
    }

    public MultiSortList(List<T> list)
    {
        this.list = new(list);
        sortIndexes = new();
        comparisons = new();
    }

    /// <summary>
    /// Add a sort order. If add an existing label, it will override the previous set order.
    /// </summary>
    public void AddSort(string sortLabel, Comparison<T> comparison)
    {
        List<int> ids = Enumerable.Range(0, Count).ToList();
        sortIndexes[sortLabel] = new(ids, Comparer<int>.Create((int x, int y) => comparison(list[x], list[y])));
        comparisons[sortLabel] = comparison;
    }

    public void AddItem(T item)
    {
        list.Add(item);
        foreach (var ids in sortIndexes.Values)
        {
            ids.Add(Count - 1);
        }
    }

    public void RemoveItem(T item)
    {
        int index = list.IndexOf(item);
        RemoveItemAt(index);
    }

    public void RemoveItemAt(int index)
    {
        list.RemoveAt(index);
        // 大概，只能重新排序了
        for (int i = 0; i < sortIndexes.Count; i++)
        {
            string sortLabel = sortIndexes.Keys.ElementAt(i);
            List<int> ids = Enumerable.Range(0, Count).ToList();
            sortIndexes[sortLabel] = new(ids, Comparer<int>.Create((int x, int y) => comparisons[sortLabel](list[x], list[y])));
        }
    }

    public void Clear()
    {
        list.Clear();
        foreach (var ids in sortIndexes.Values) ids.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in list)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
