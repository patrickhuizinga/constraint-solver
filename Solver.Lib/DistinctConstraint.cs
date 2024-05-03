namespace Solver.Lib;

public class DistinctConstraint : IConstraint
{
    private readonly int[] _variableIndices;
    
    /// <summary>
    /// If this value is not-null, then it represents the value multiple variables may have at the same time.
    /// Otherwise; each variable must have a unique value.
    /// </summary>
    public int? DefaultValue { get; init; }

    public DistinctConstraint(params Variable[] variables)
    {
        _variableIndices = new int[variables.Length];
        for (int i = 0; i < variables.Length; i++)
        {
            _variableIndices[i] = variables[i].Index;
        }
    }
    
    public RestrictResult Restrict(IList<VariableType> variables)
    {
        var result = RestrictResult.NoChange;
        
        foreach (var i in _variableIndices)
        {
            if (!variables[i].TryGetConstant(out int value)) continue;
            
            if (value == DefaultValue) continue;
            
            foreach (var j in _variableIndices)
            {
                if (i == j) continue;

                var elResult = Variable.Exclude(j, value, variables);
                if (elResult == RestrictResult.Infeasible)
                    return RestrictResult.Infeasible;

                if (result == RestrictResult.NoChange)
                    result = elResult;
            }
        }

        return result;
    }

    public int Range(IList<VariableType> variables)
    {
        return _variableIndices.Sum(i => variables[i].Max - variables[i].Min);
    }

    public IEnumerable<int> GetVariableIndices()
    {
        return _variableIndices;
    }
}