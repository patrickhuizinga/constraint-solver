using System.Collections;

namespace Solver.Lib;

public sealed class VariableCollection : IList<VariableType>
{
    private readonly List<VariableType> _items = [];
    private readonly List<int> _modifiedIndices = [];

    public VariableCollection()
    {
    }

    public VariableCollection(IEnumerable<VariableType> items)
    {
        _items.AddRange(items);
    }
        
    public void AddRange(IEnumerable<VariableType> items)
    {
        _items.AddRange(items);
    }
        
    public IEnumerator<VariableType> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }

    public void Add(VariableType item)
    {
        _items.Add(item);
    }

    public void Clear()
    {
        _items.Clear();
        _modifiedIndices.Clear();
    }

    public bool Contains(VariableType item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(VariableType[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public bool Remove(VariableType item)
    {
        throw new NotSupportedException();
    }

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public int IndexOf(VariableType item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, VariableType item)
    {
        throw new NotSupportedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    public VariableType this[int index]
    {
        get => _items[index];
        set
        {
            _items[index] = value;
            _modifiedIndices.Add(index);
        }
    }

    public IEnumerable<int> GetModifications()
    {
        return _modifiedIndices;
    }

    public void ClearModifications()
    {
        _modifiedIndices.Clear();
    }
}
