using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Square.Models;

namespace SquareHackathonWPF.Models.SquareApi;

public class ItemList : IList<CatalogObject>
{
    private List<CatalogObject> Items { get; } = new();
    public IEnumerator<CatalogObject> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Items).GetEnumerator();

    public void Add(CatalogObject item) { if (item.Type == "ITEM") Items.Add(item); }

    public void Clear() => Items.Clear();

    public bool Contains(CatalogObject item) => Items.Contains(item);

    public void CopyTo(CatalogObject[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public bool Remove(CatalogObject item) => Items.Remove(item);

    public int Count => Items.Count;

    public bool IsReadOnly => ((ICollection<CatalogObject>) Items).IsReadOnly;

    public int IndexOf(CatalogObject item) => Items.IndexOf(item);

    public void Insert(int index, CatalogObject item) => Items.Insert(index, item);

    public void RemoveAt(int index) => Items.RemoveAt(index);

    public CatalogObject this[int index] {
        get => Items[ index];
        set => Items[ index] = value;
    }

    public CatalogObject? Find(Predicate<CatalogObject> func) => Items.Find(func);

    public int FindIndex(Predicate<CatalogObject> func) => Items.FindIndex(func);

    public int FindIndex(int startIndex, Predicate<CatalogObject> func) => Items.FindIndex(startIndex, func);

    public int FindIndex(int startIndex, int count, Predicate<CatalogObject> func) => Items.FindIndex(startIndex, count, func);
}