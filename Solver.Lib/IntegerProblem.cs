namespace Solver.Lib;

public class IntegerProblem
{
    private readonly Dictionary<Variable, Variable> _variables = new();
    private readonly List<Constraint> _constraints = [];

    public Variable this[Variable variable]
    {
        get => _variables[variable];
        set => _variables[variable] = value;
    }

    public bool IsSolved => _variables.Values.All(value => value is ConstantVariable);

    public BinaryVariable AddBinaryVariable()
    {
        var bv = new BinaryVariable();
        _variables.Add(bv, bv);
        return bv;
    }

    public BinaryVariable[] AddBinaryVariables(int count)
    {
        var result = new BinaryVariable[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = AddBinaryVariable();
        }

        return result;
    }

    public BinaryVariable[,] AddBinaryVariables(int countI, int countJ)
    {
        var result = new BinaryVariable[countI, countJ];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            result[i, j] = AddBinaryVariable();
        }

        return result;
    }

    public BinaryVariable[,,] AddBinaryVariables(int countI, int countJ, int countK)
    {
        var result = new BinaryVariable[countI, countJ, countK];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        for(int k = 0; k < countK; k++)
        {
            result[i, j, k] = AddBinaryVariable();
        }

        return result;
    }

    public Constraint AddConstraint(Expression left, Comparison comparison, Expression right)
    {
        var c = new Constraint(left, comparison, right);
        _constraints.Add(c);
        return c;
    }

    public Constraint AddConstraint(Expression left, Comparison comparison, int right)
    {
        return AddConstraint(left, comparison, new ConstantVariable(right));
    }

    public void AddConstraint(Constraint constraint)
    {
        _constraints.Add(constraint);
    }

    public Constraint[] AddConstraints(Expression[] left, Comparison comparison, int right)
    {
        var result = new Constraint[left.Length];
        for (int i = 0; i < left.Length; i++)
        {
            result[i] = AddConstraint(left[i], comparison, right);
        }

        return result;
    }

    public Constraint[,] AddConstraints(Expression[,] left, Comparison comparison, int right)
    {
        var countI = left.GetLength(0);
        var countJ = left.GetLength(1);
        
        var result = new Constraint[countI, countJ];
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

        foreach (var variable in worstConstraint.GetVariables())
        {
            var value = _variables[variable];
            if (value is ConstantVariable)
                continue;

            for (int i = value.Min; i <= value.Max; i++)
            {
                var clone = Clone();
                clone[variable] = i;

                if (clone.FindFeasible())
                {
                    foreach (var pair in clone._variables)
                        _variables[pair.Key] = pair.Value;
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    private Constraint? GetWorstConstraint()
    {
        Constraint? result = null;
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

    public IntegerProblem Clone()
    {
        var result = new IntegerProblem();
        result._constraints.AddRange(_constraints);
        foreach (var pair in _variables)
            result._variables[pair.Key] = pair.Value;

        return result;
    }
}