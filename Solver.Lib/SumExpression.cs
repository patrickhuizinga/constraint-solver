using System.Diagnostics;

namespace Solver.Lib;

public sealed class SumExpression : Expression
{
    private readonly SortedList<int, int> _variables;
    private readonly int _constant;

    public override int Constant => _constant;

    public SumExpression(Expression expression, Expression addition, int scale)
    {
        _variables = new SortedList<int, int>();
        AddVariables(expression, 1, ref _constant);
        AddVariables(addition, scale, ref _constant);
    }

    public SumExpression(Expression expression, Variable variable, int scale)
    {
        _variables = new SortedList<int, int> { { variable.Index, scale } };
        AddVariables(expression, 1, ref _constant);
    }

    public SumExpression(params Variable[] variables)
    {
        _variables = new SortedList<int, int>();
        foreach (var v in variables) 
            AddVariable(v.Index, 1);
    }

    private SumExpression(SumExpression source, int constant)
    {
        _variables = new SortedList<int, int>(source._variables);
        _constant = source._constant + constant;
    }

    private SumExpression(SumExpression source, Expression addition, int scale)
    {
        _variables = new SortedList<int, int>(source._variables);
        _constant = source._constant;
        
        AddVariables(addition, scale, ref _constant);
    }

    private SumExpression(SumExpression source, Variable addition, int scale)
    {
        _variables = new SortedList<int, int>(source._variables);
        _constant = source._constant;
        
        AddVariable(addition.Index, scale);
    }

    private void AddVariables(Expression expression, int scale, ref int constant)
    {
        constant += scale * expression.Constant;
        
        foreach (var (i, s) in expression.GetVariables()) 
            AddVariable(i, scale * s);
    }

    private void AddVariable(int index, int scale)
    {
        var i = _variables.IndexOfKey(index);

        if (i == -1)
            _variables[index] = scale;
        else
            _variables.SetValueAtIndex(i, _variables.GetValueAtIndex(i) + scale);
    }

    public static Expression Create()
    {
        return new ConstantExpression(0);
    }

    public static Expression Create(Variable variable)
    {
        return new Add1Expression(variable.Index, 1, 0);
    }

    public static Expression Create(Variable variable1, Variable variable2)
    {
        if (variable1.Index == variable2.Index)
            return new Add1Expression(variable1.Index, 2, 0);

        return new Add2Expression(variable1.Index, 1, variable2.Index, 1, 0);
    }

    public static Expression Create(params Variable[] variables)
    {
        return variables.Length switch
        {
            0 => new ConstantExpression(0),
            1 => new Add1Expression(variables[0].Index, 1, 0),
            2 => Create(variables[0], variables[1]),
            _ => new SumExpression(variables)
        };
    }

    public override SumExpression Add(Expression addition, int scale)
    {
        return new SumExpression(this, addition, scale);
    }

    public override SumExpression Add(int addition)
    {
        return new SumExpression(this, addition);
    }

    public override Expression Add(Variable addition, int scale)
    {
        return new SumExpression(this, addition, scale);
    }

    public override int GetMin(IList<VariableType> variables)
    {
        return Constant + _variables.Sum(pair => variables[pair.Key].GetMin(pair.Value));
    }

    public override int GetMax(IList<VariableType> variables)
    {
        return Constant + _variables.Sum(pair => variables[pair.Key].GetMax(pair.Value));
    }

    public override RestrictResult RestrictToMin(int minValue, IList<VariableType> variables)
    {
        int maxSum = Constant;
        foreach(var (index, scale) in _variables) 
            maxSum += variables[index].GetMax(scale);

        if (maxSum < minValue)
            return RestrictResult.Infeasible;

        var diff = maxSum - minValue;

        var result = RestrictResult.NoChange;
        foreach(var (index, scale) in _variables)
        {
            if (scale == 0)
                continue;
            
            var variable = variables[index];
            var elDiff = diff / scale;

            // note that ellDiff will be negative when scale is negative
            var elResult = scale > 0
                ? Variable.RestrictToMin(index, variable.Max - elDiff, variables)
                : Variable.RestrictToMax(index, variable.Min - elDiff, variables);
            
            if (elResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a lower minimum bound than the max possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            if (result == RestrictResult.NoChange) 
                result = elResult;
        }

        return result;
    }

    public override RestrictResult RestrictToMax(int maxValue, IList<VariableType> variables)
    {
        int minSum = Constant;
        foreach(var (index, scale) in _variables) 
            minSum += variables[index].GetMin(scale);

        if (maxValue < minSum)
            return RestrictResult.Infeasible;

        var diff = maxValue - minSum;

        var result = RestrictResult.NoChange;
        foreach(var (index, scale) in _variables)
        {
            if (scale == 0)
                continue;
            
            var variable = variables[index];
            var elDiff = diff / scale;

            // note that elDiff will be negative when scale is negative
            var elResult = scale > 0
                ? Variable.RestrictToMax(index, variable.Min + elDiff, variables)
                : Variable.RestrictToMin(index, variable.Max + elDiff, variables);
            
            if (elResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a higher maximum bound than the min possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            if (result == RestrictResult.NoChange)
                result = elResult;
        }

        return result;
    }

    public override IEnumerable<int> GetVariableIndices()
    {
        return _variables.Keys;
    }

    public override IEnumerable<KeyValuePair<int, int>> GetVariables()
    {
        return _variables;
    }
}