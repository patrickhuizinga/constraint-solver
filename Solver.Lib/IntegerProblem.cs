namespace Solver.Lib;

public class IntegerProblem
{
    private readonly VariableCollection _variables;
    private readonly List<IConstraint> _constraints;
    private readonly List<List<int>> _variableToConstraint;

    public IntegerProblem()
    {
        _variables = new VariableCollection();
        _constraints = [];
        _variableToConstraint = [];
    }

    public IntegerProblem(IntegerProblem source)
    {
        _variables = new VariableCollection(source._variables);
        _constraints = source._constraints;
        _variableToConstraint = source._variableToConstraint;
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

    public bool IsSolved => _variables.All(value => value.IsConstant);

    public Variable AddVariable(VariableType variable)
    {
        var index = _variables.Count;
        _variables.Add(variable);
        _variableToConstraint.Add([]);
        return new Variable(index);
    }

    public Variable[] AddVariables(VariableType variable, int count)
    {
        var result = new Variable[count];

        for (int i = 0; i < count; i++)
            result[i] = AddVariable(variable);

        return result;
    }

    public Variable[,] AddVariables(VariableType variable, int countI, int countJ)
    {
        var result = new Variable[countI, countJ];

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
            result[i, j] = AddVariable(variable);

        return result;
    }

    public Variable[,,] AddVariables(VariableType variable, int countI, int countJ, int countK)
    {
        var result = new Variable[countI, countJ, countK];

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        for (int k = 0; k < countK; k++)
        {
            result[i, j, k] = AddVariable(variable);
        }

        return result;
    }

    public Variable AddBinaryVariable() => AddVariable(VariableType.Binary);

    public Variable[] AddBinaryVariables(int count) => AddVariables(VariableType.Binary, count);

    public Variable[,] AddBinaryVariables(int countI, int countJ) => AddVariables(VariableType.Binary, countI, countJ);

    public Variable[,,] AddBinaryVariables(int countI, int countJ, int countK) =>
        AddVariables(VariableType.Binary, countI, countJ, countK);

    public void AddConstraint(IConstraint constraint)
    {
        int constraintIndex = _constraints.Count;
        _constraints.Add(constraint);

        foreach (var index in constraint.GetVariableIndices())
            _variableToConstraint[index].Add(constraintIndex);
    }

    public void AddConstraints(IEnumerable<IConstraint> constraints)
    {
        foreach (var constraint in constraints)
            AddConstraint(constraint);
    }

    public void AddConstraints<TConstraint>(TConstraint[,] constraints) where TConstraint : IConstraint
    {
        var countI = constraints.GetLength(0);
        var countJ = constraints.GetLength(1);

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            AddConstraint(constraints[i, j]);
        }
    }

    public void AddConstraint(Expression left, Comparison comparison, Expression right)
    {
        AddConstraint(
            Constraint.Create(left, comparison, right));
    }

    public void AddConstraints<TExpression>(
        TExpression[] left, Comparison comparison, Expression right)
        where TExpression : Expression
    {
        foreach (var leftItem in left) 
            AddConstraint(leftItem, comparison, right);
    }

    public void AddConstraints<TExpression>(
        TExpression[,] left, Comparison comparison, Expression right)
        where TExpression : Expression
    {
        var countI = left.GetLength(0);
        var countJ = left.GetLength(1);

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            AddConstraint(left[i, j], comparison, right);
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
            var constraintResult = constraint.Restrict(_variables);
            if (constraintResult == RestrictResult.Infeasible)
                return RestrictResult.Infeasible;

            if (constraintResult == RestrictResult.Change)
                result = RestrictResult.Change;
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
            var clone = new IntegerProblem(this);
            clone._variables[variableIndex] = value;

            if (clone.FindFeasible())
            {
                _variables.Clear();
                _variables.AddRange(clone._variables);
                return true;
            }
        }

        return false;
    }

    private int GetSmallestVariable()
    {
        // By returning the variable with the least 'range', we have fewer options to consider
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
}