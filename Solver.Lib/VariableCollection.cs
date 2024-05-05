using System.Collections;

namespace Solver.Lib;

public sealed class VariableCollection : IReadOnlyList<VariableType>
{
    private readonly List<VariableType> _values = [];
    private readonly List<int> _modifiedIndices = [];

    public VariableCollection()
    {
    }

    public VariableCollection(VariableCollection items)
    {
        _values.AddRange(items._values);
    }
    
    public VariableType this[int index]
    {
        get => _values[index];
        set
        {
            _values[index] = value;
            _modifiedIndices.Add(index);
        }
    }

    public int Count => _values.Count;

    public void Add(VariableType item)
    {
        _values.Add(item);
    }

    public void CopyFrom(VariableCollection source)
    {
        _values.Clear();
        _values.AddRange(source._values);
        _modifiedIndices.Clear();
    }

    public IEnumerable<int> GetModifications()
    {
        return _modifiedIndices;
    }

    public void ClearModifications()
    {
        _modifiedIndices.Clear();
    }
        
    public IEnumerator<VariableType> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
