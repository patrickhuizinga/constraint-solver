namespace Solver.Lib;

public class CompoundType : VariableType
{
    private readonly List<VariableType> _variables;

    private CompoundType(List<VariableType> variables)
    {
        _variables = variables;
        Min = _variables.Min(v => v.Min);
        Max = _variables.Max(v => v.Max);
    }

    public static VariableType Create(params VariableType[] variables)
    {
        var list = variables
            .SelectMany(Decompose)
            .OrderBy(v => v.Min)
            .ToList();

        if (list.Count == 1)
            return list[0];

        var result = new List<VariableType>();
        foreach (var variable in list)
        {
            if (result.Count == 0)
            {
                result.Add(variable);
                continue;
            }

            var last = result[^1];
            if (last.Max >= variable.Max) continue;

            if (last.Max + 1 >= variable.Min)
                result[^1] = RangeType.Create(last.Min, variable.Max);
            else
                result.Add(variable);
        }

        if (result.Count == 1)
            return result[0];

        return new CompoundType(result);
    }

    private static IEnumerable<VariableType> Decompose(VariableType variable)
    {
        if (variable is CompoundType compound)
            return compound._variables;

        return [variable];
    }

    public override int Min { get; }

    public override int Max { get; }

    public override VariableType? TryRestrictToMin(int minValue)
    {
        if (minValue <= Min)
            return this;
        if (minValue == Max)
            return new ConstantType(minValue);
        if (Max < minValue)
            return null;

        for (int i = 0; i < _variables.Count; i++)
        {
            var variable = _variables[i];
            if (variable.Max < minValue) continue;
            
            if (minValue <= variable.Min)
            {
                if (i == _variables.Count - 1)
                    return variable;

                return new CompoundType(_variables[i..^1] );
            }
            
            // variable.Min < minValue <= variable.Max

            var newVariable = variable.TryRestrictToMin(minValue);

            if (i == _variables.Count - 1)
                return newVariable;

            if (ReferenceEquals(newVariable, null) && i == _variables.Count - 2) 
                return _variables[^1];

            List<VariableType> newVariables;
            switch (newVariable)
            {
                case null:
                    newVariables = _variables[(i+1)..^1];
                    break;
                case CompoundType cv:
                    newVariables = _variables[(i+1)..^1];
                    newVariables.InsertRange(0, cv._variables);
                    break;
                default:
                    newVariables = _variables[i..^1];
                    newVariables[0] = newVariable;
                    break;
            }
            return new CompoundType(newVariables);
        }

        return null;
    }

    public override VariableType? TryRestrictToMax(int maxValue)
    {
        if (Max <= maxValue)
            return this;
        if (maxValue == Min)
            return new ConstantType(maxValue);
        if (maxValue < Min)
            return null;

        for (int i = 0; i < _variables.Count; i++)
        {
            var variable = _variables[i];
            if (variable.Max <= maxValue) continue;
            
            if (maxValue < variable.Min)
            {
                if (i == 1)
                    return _variables[0];

                return new CompoundType(_variables[0..i] );
            }
            
            // variable.Min <= maxValue < variable.Max

            var newVariable = variable.TryRestrictToMax(maxValue);

            if (i == 0)
                return newVariable;

            if (ReferenceEquals(newVariable, null) && i == 1) 
                return _variables[0];

            List<VariableType> newVariables;
            switch (newVariable)
            {
                case null:
                    newVariables = _variables[0..(i-1)];
                    break;
                case CompoundType cv:
                    newVariables = _variables[0..(i-1)];
                    newVariables.AddRange(cv._variables);
                    break;
                default:
                    newVariables = _variables[0..i];
                    newVariables[^1] = newVariable;
                    break;
            }
            return new CompoundType(newVariables);
        }

        return null;
    }

    public override VariableType TryExclude(int value)
    {
        if (value < Min || Max < value)
            return this;

        for (int i = 0; i < _variables.Count; i++)
        {
            var variable = _variables[i];
            if (variable.Max < value) continue;
            if (value < variable.Min) break;

            var newVariable = variable.TryExclude(value);
            if (ReferenceEquals(newVariable, variable))
                return this;

            if (ReferenceEquals(newVariable, null) && _variables.Count == 2)
            {
                // return the other variable
                return _variables[1 - i];
            }

            var newVariables = new List<VariableType>(_variables);
            switch (newVariable)
            {
                case null:
                    newVariables.RemoveAt(i);
                    break;
                case CompoundType cv:
                    newVariables.RemoveAt(i);
                    newVariables.InsertRange(i, cv._variables);
                    break;
                default:
                    newVariables[i] = newVariable;
                    break;
            }
            return new CompoundType(newVariables);
        }

        return this;
    }

    public override string ToString()
    {
        return String.Join('+', _variables);
    }
}