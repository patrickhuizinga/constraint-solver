namespace Solver.Lib;

public class DistinctConstraint : IConstraint
{
    private readonly Variable[] _variables;
    
    /// <summary>
    /// If this value is not-null, then it represents the value multiple variables may have at the same time.
    /// Otherwise; each variable must have a unique value.
    /// </summary>
    public int? DefaultValue { get; init; }

    public DistinctConstraint(params Variable[] variables)
    {
        _variables = variables;
    }
    
    public RestrictResult Restrict(Dictionary<Variable, Variable> variables)
    {
        var result = RestrictResult.NoChange;
        
        for (var i = 0; i < _variables.Length; i++)
        {
            var variable = variables[_variables[i]];
            if (variable is not ConstantVariable cv) continue;
            
            if (cv.Value == DefaultValue) continue;
            
            for (int j = 0; j < _variables.Length; j++)
            {
                if (i == j) continue;

                var elResult = _variables[j].Exclude(cv.Value, variables);
                if (elResult == RestrictResult.Infeasible)
                    return RestrictResult.Infeasible;

                if (result == RestrictResult.NoChange)
                    result = elResult;
            }
        }

        return result;
    }

    public int Range(Dictionary<Variable, Variable> variables)
    {
        return _variables.Sum(v => v.GetMax(variables) - v.GetMin(variables));
    }

    public IEnumerable<Variable> GetVariables()
    {
        return _variables;
    }
}