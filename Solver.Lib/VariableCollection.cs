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

    public double GetMin(int index, double scale) => this[index].GetMin(scale);
    public double GetMin(KeyValuePair<int, double> indexAndScale) => GetMin(indexAndScale.Key, indexAndScale.Value);

    public double GetMax(int index, double scale) => this[index].GetMax(scale);
    public double GetMax(KeyValuePair<int, double> indexAndScale) => GetMax(indexAndScale.Key, indexAndScale.Value);
    

    public void Add(VariableType item)
    {
        _values.Add(item);
    }

    public void Add(int min, int max)
    {
        _values.Add(new VariableType(min, max));
    }

    public RestrictResult RestrictToMin(int index, int minValue)
    {
        return Variable.RestrictToMin(index, minValue, this);
    }

    public RestrictResult RestrictToMax(int index, int maxValue)
    {
        return Variable.RestrictToMax(index, maxValue, this);
    }

    public RestrictResult RestrictToRange(int index, int minValue, int maxValue)
    {
        var resultMin = Variable.RestrictToMin(index, minValue, this);
        if (resultMin == RestrictResult.Infeasible)
            return RestrictResult.Infeasible;
        
        var resultMax = Variable.RestrictToMax(index, maxValue, this);
        
        return resultMax == RestrictResult.NoChange ? resultMin : resultMax;
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
