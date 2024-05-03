using System.Collections;

namespace Solver.Lib;

public class EqualityProblem
{
    private readonly ModifyTrackerList<VariableType> _variables;
    private readonly List<EqualityConstraint> _constraints;
    private readonly List<List<int>> _variableToConstraint;

    public EqualityProblem()
    {
        _variables = new ModifyTrackerList<VariableType>();
        _constraints = [];
        _variableToConstraint = [];
        
        _variables.Add(1);
        _variableToConstraint.Add([]);
    }

    public EqualityProblem(EqualityProblem source)
    {
        _variables = new ModifyTrackerList<VariableType>(source._variables);
        _constraints = source._constraints
            // .Select(constraint => constraint.Clone())
            // .ToList()
            ;
        _variableToConstraint = source._variableToConstraint
            .Select(Enumerable.ToList)
            .ToList();
    }

    public VariableType this[Variable variable]
    {
        get => _variables[variable.Index];
        set => _variables[variable.Index] = value;
    }

    public VariableType[] this[Variable[] variables]
    {
        get
        {
            var result = new VariableType[variables.Length];
            for (int i = 0; i < variables.Length; i++)
            {
                result[i] = this[variables[i]];
            }

            return result;
        }
    }

    public VariableType[,] this[Variable[,] variables]
    {
        get
        {
            var lengthI = variables.GetLength(0);
            var lengthJ = variables.GetLength(1);

            var result = new VariableType[lengthI, lengthJ];
            for (int i = 0; i < lengthI; i++)
            for (int j = 0; j < lengthJ; j++)
            {
                result[i, j] = this[variables[i, j]];
            }

            return result;
        }
    }

    public VariableType[,,] this[Variable[,,] variables]
    {
        get
        {
            var lengthI = variables.GetLength(0);
            var lengthJ = variables.GetLength(1);
            var lengthK = variables.GetLength(2);

            var result = new VariableType[lengthI, lengthJ, lengthK];
            for (int i = 0; i < lengthI; i++)
            for (int j = 0; j < lengthJ; j++)
            for (int k = 0; k < lengthK; k++)
            {
                result[i, j, k] = this[variables[i, j, k]];
            }

            return result;
        }
    }

    public bool IsSolved => _variables.All(value => value is ConstantType);

    public Variable AddVariable(VariableType variable)
    {
        var index = _variables.Count;
        _variables.Add(variable);
        _variableToConstraint.Add([]);
        return new Variable(index);
    }

    public Variable AddBinaryVariable()
    {
        return AddVariable(
            new BinaryType());
    }

    public Variable[] AddBinaryVariables(int count)
    {
        var result = new Variable[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = AddBinaryVariable();
        }

        return result;
    }

    public Variable[,] AddBinaryVariables(int countI, int countJ)
    {
        var result = new Variable[countI, countJ];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            result[i, j] = AddBinaryVariable();
        }

        return result;
    }

    public Variable[,,] AddBinaryVariables(int countI, int countJ, int countK)
    {
        var result = new Variable[countI, countJ, countK];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        for (int k = 0; k < countK; k++)
        {
            result[i, j, k] = AddBinaryVariable();
        }

        return result;
    }

    public Variable AddRangeVariable(Range range)
    {
        var min = range.Start.Value;
        var max = range.End.Value;

        return AddVariable(RangeType.Create(min, max));
    }

    public Variable[] AddRangeVariables(Range range, int count)
    {
        var min = range.Start.Value;
        var max = range.End.Value;

        var result = new Variable[count];

        for (int i = 0; i < count; i++)
            result[i] = AddVariable(RangeType.Create(min, max));

        return result;
    }

    public Variable[,] AddRangeVariables(Range range, int countI, int countJ)
    {
        var min = range.Start.Value;
        var max = range.End.Value;

        var result = new Variable[countI, countJ];

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
            result[i, j] = AddVariable(RangeType.Create(min, max));

        return result;
    }

    public void AddConstraints<TExpression>(
        TExpression[,] left, int right)
        where TExpression : Expression
    {
        var countI = left.GetLength(0);
        var countJ = left.GetLength(1);

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            var constraintIndex = _constraints.Count;
            var constraint = new EqualityConstraint(left[i, j], right);
            _constraints.Add(constraint);

            foreach (var index in constraint.GetVariableIndices())
            {
                _variableToConstraint[index].Add(constraintIndex);
            }
        }
    }

    public RestrictResult Restrict()
    {
        RestrictResult finalResult = RestrictResult.NoChange;
        while (true)
        {
            var result = RestrictOnce();

            switch (result)
            {
                case RestrictResult.Infeasible:
                    return RestrictResult.Infeasible;
                case RestrictResult.NoChange:
                    return finalResult;
                case RestrictResult.Change:
                    finalResult = RestrictResult.Change;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "unknown result " + result);
            }
        }
    }

    private RestrictResult RestrictOnce()
    {
        var result = RestrictResult.NoChange;

        var modifiedVariables = _variables.GetModifications()
            .Distinct()
            .ToList();
        _variables.ClearModifications();
        var modifiedConstraints = modifiedVariables
            .SelectMany(varIndex => _variableToConstraint[varIndex])
            .Distinct();

        if (modifiedVariables.Count == 0)
        {
            modifiedConstraints = Enumerable.Range(0, _constraints.Count);
        }

        foreach (var constraint in modifiedConstraints.Select(i => _constraints[i]))
        {
            switch (constraint.Restrict(_variables))
            {
                case RestrictResult.Infeasible:
                    return RestrictResult.Infeasible;
                case RestrictResult.Change:
                    result = RestrictResult.Change;
                    break;
                case RestrictResult.NoChange:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var variableIndex in modifiedVariables)
        {
            if (_variables[variableIndex] is ConstantType)
                _variableToConstraint[variableIndex].Clear();
        }
        
        return result;
    }

    public bool FindFeasible()
    {
        var result = Restrict();
        if (result == RestrictResult.Infeasible)
            return false;

        if (IsSolved)
            return true;

        var variableIndex = GetSmallestVariable();
        if (variableIndex == -1)
            throw new InvalidOperationException("There is no worst constraint?");

        var variable = _variables[variableIndex];

        // Larger values are more likely be decisive, probably
        for (int value = variable.Max; value >= variable.Min; value--)
        {
            var clone = new EqualityProblem(this);
                
            clone._variables[variableIndex] = value;
            
            if (clone.FindFeasible())
            {
                _variables.Clear();
                _variables.AddRange(clone._variables);
                _constraints.Clear();
                _variableToConstraint.Clear();
                return true;
            }
        }

        return false;
    }

    private int GetSmallestVariable()
    {
        // By returning the variable with he least 'range', we have fewer options to consider
        // and thereby need fewer guesses to make progress.
        
        int index = -1;
        int smallestDiff = int.MaxValue;
        for (int i = 0; i < _variables.Count; i++)
        {
            var variable = _variables[i];
            int diff = variable.Max - variable.Min;
            
            if (diff <= 0 || smallestDiff <= diff) continue;
            
            // can't get smaller than this
            if (diff == 1) return i;
            
            smallestDiff = diff;
            index = i;
        }

        return index;
    }


    private sealed class ModifyTrackerList<T> : IList<T>
    {
        private readonly List<T> _items = [];
        private readonly List<int> _modifiedIndices = [];

        public ModifyTrackerList()
        {
        }

        public ModifyTrackerList(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }
        
        public void AddRange(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
            _modifiedIndices.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public T this[int index]
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
}
