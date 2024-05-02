namespace Solver.Lib;

public class IntegerProblem
{
    private readonly List<VariableType> _variables;
    private readonly List<IConstraint> _constraints;

    public IntegerProblem()
    {
        _variables = [1];
        _constraints = [];
    }

    public IntegerProblem(IntegerProblem source)
    {
        _variables = [..source._variables];
        _constraints = [..source._constraints];
    }

    public VariableType this[Variable variable]
    {
        get => _variables[variable.Index];
        set => _variables[variable.Index] = value;
    }

    public bool IsSolved => _variables.All(value => value is ConstantType);

    public Variable AddVariable(VariableType variable)
    {
        var index = _variables.Count;
        _variables.Add(variable);
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

    public TConstraint AddConstraint<TConstraint>(TConstraint constraint) where TConstraint : IConstraint
    {
        _constraints.Add(constraint);
        return constraint;
    }

    public void AddConstraints(IEnumerable<IConstraint> constraints)
    {
        foreach (var constraint in constraints)
        {
            AddConstraint(constraint);
        }
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

    public ComparisonConstraint AddConstraint(Expression left, Comparison comparison, Expression right)
    {
        return AddConstraint(
            new ComparisonConstraint(left, comparison, right));
    }

    public ComparisonConstraint[] AddConstraints<TExpression>(
        TExpression[] left, Comparison comparison, Expression right)
        where TExpression : Expression
    {
        var result = new ComparisonConstraint[left.Length];
        for (int i = 0; i < left.Length; i++)
        {
            result[i] = AddConstraint(left[i], comparison, right);
        }

        return result;
    }

    public ComparisonConstraint[,] AddConstraints<TExpression>(
        TExpression[,] left, Comparison comparison, Expression right)
        where TExpression : Expression
    {
        var countI = left.GetLength(0);
        var countJ = left.GetLength(1);

        var result = new ComparisonConstraint[countI, countJ];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            result[i, j] = AddConstraint(left[i, j], comparison, right);
        }

        return result;
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

        foreach (var constraint in _constraints)
        {
            var constraintResult = constraint.Restrict(_variables);
            if (constraintResult == RestrictResult.Infeasible)
                return RestrictResult.Infeasible;

            if (result == RestrictResult.NoChange)
                result = constraintResult;
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

        var worstConstraint = GetWorstConstraint();
        if (worstConstraint == null)
            throw new InvalidOperationException("There is no worst constraint?");

        foreach (var varIndex in worstConstraint.GetVariableIndices())
        {
            var value = _variables[varIndex];
            if (value is ConstantType)
                continue;

            for (int i = value.Min; i <= value.Max; i++)
            {
                var clone = new IntegerProblem(this);
                clone._variables[varIndex] = i;

                if (clone.FindFeasible())
                {
                    _variables.Clear();
                    _variables.AddRange(clone._variables);
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    private IConstraint? GetWorstConstraint()
    {
        IConstraint? result = null;
        int worstRange = int.MaxValue;
        foreach (var constraint in _constraints)
        {
            var range = constraint.Range(_variables);
            if (range == 0) continue;
            if (worstRange <= range) continue;

            result = constraint;
            worstRange = range;
        }

        return result;
    }
}