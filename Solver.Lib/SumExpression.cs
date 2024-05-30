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

    public SumExpression(params Expression[] expressions)
    {
        _variables = new SortedList<int, int>();
        foreach (var expression in expressions) 
            AddVariables(expression, 1, ref _constant);
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

    public SumExpression(IEnumerable<Variable> variables, IEnumerable<int> scales, int constant)
        :this(variables.Zip(scales), constant)
    {
    }

    public SumExpression(IEnumerable<(Variable, int scale)> variables, int constant)
    {
        _variables = new SortedList<int, int>();
        _constant = constant;
        
        foreach (var (variable, scale) in variables) 
            AddVariable(variable.Index, scale);
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

    private SumExpression(SortedList<int, int> variables, int constant)
    {
        _variables = variables;
        _constant = constant;
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
        {
            _variables.Add(index, scale);
            return;
        }

        var newScale = _variables.GetValueAtIndex(i) + scale;
        if (newScale == 0)
            _variables.RemoveAt(i);
        else
            _variables.SetValueAtIndex(i, newScale);
    }

    public static Expression Create()
    {
        return new ConstantExpression(0);
    }

    public static Expression Create(Variable variable)
    {
        return new VariableExpression(variable.Index, 1, 0);
    }

    public static Expression Create(Variable variable1, Variable variable2)
    {
        if (variable1.Index == variable2.Index)
            return new VariableExpression(variable1.Index, 2, 0);

        return new SumExpression(variable1, variable2);
    }

    public static Expression Create(params Variable[] variables)
    {
        return variables.Length switch
        {
            0 => new ConstantExpression(0),
            1 => new VariableExpression(variables[0].Index, 1, 0),
            2 => Create(variables[0], variables[1]),
            _ => new SumExpression(variables)
        };
    }

    public static Expression Create(params Expression[] expressions)
    {
        return expressions.Length switch
        {
            0 => new ConstantExpression(0),
            1 => expressions[0],
            _ => new SumExpression(expressions)
        };
    }

    public override SumExpression Add(Expression addition, int scale)
    {
        if (scale == 0)
            return this;

        if (addition is ConstantExpression cv)
            return Add(scale * cv.Value);
        
        return new SumExpression(this, addition, scale);
    }

    public override SumExpression Add(int addition)
    {
        if (addition == 0)
            return this;
        
        return new SumExpression(this, addition);
    }

    public override Expression Add(Variable addition, int scale)
    {
        if (scale == 0)
            return this;

        return new SumExpression(this, addition, scale);
    }

    public override int GetScale(int variableIndex) => _variables.GetValueOrDefault(variableIndex);
    
    public override Expression EliminateConstants(VariableCollection variables)
    {
        int newConstant = _constant;
        SortedList<int, int>? newVariables = null;
        
        for (int i = _variables.Count - 1; i >= 0; i--)
        {
            var index = _variables.GetKeyAtIndex(i);
            if (variables[index].TryGetConstant(out var value))
            {
                newVariables ??= new SortedList<int, int>(_variables);
                
                var scale = _variables.GetValueAtIndex(i);
                newConstant += scale * value;
                newVariables.RemoveAt(i);
            }
        }

        if (newVariables == null)
            return this;
        
        return new SumExpression(newVariables, newConstant);
    }

    public override VariableType GetRange(VariableCollection variables)
    {
        return _variables.Aggregate(
            (VariableType)Constant,
            (range, pair) => range + variables[pair.Key] * pair.Value);
    }

    public override RestrictResult RestrictToEqualZero(VariableCollection variables)
    {
        int maxSize = 0;
        VariableType sum = _constant;
        foreach (var (index, scale) in _variables)
        {
            var scaledValue = scale * variables[index];
            sum += scaledValue;
            
            if (scaledValue.Size > maxSize)
                maxSize = scaledValue.Size;
        }

        if (!sum.Contains(0))
            return RestrictResult.Infeasible;
        if (sum.IsConstant)
            return RestrictResult.Complete;

        if (maxSize <= -sum.Min && maxSize <= sum.Max)
            return RestrictResult.NoChange;

        foreach (var (index, scale) in _variables)
        {
            if (scale == 0) continue;

            // note: sum.Min <= 0
            var elMinDiff = scale > 0
                ? sum.Min / scale
                : sum.Max / scale;

            var elMaxDiff = scale > 0
                ? sum.Max / scale
                : sum.Min / scale;

            // note: elMinDiff <= 0
            var maxValue = variables[index].Min - elMinDiff;
            var minValue = variables[index].Max - elMaxDiff;

            var elResult = variables.RestrictToRange(index, minValue, maxValue);
            if (elResult == RestrictResult.Infeasible)
                return RestrictResult.Infeasible;
        }

        if (sum.Min == 0 || sum.Max == 0)
            return RestrictResult.Complete;
        
        return RestrictResult.Change;
    }

    public override RestrictResult RestrictToMaxZero(VariableCollection variables)
    {
        int maxSize = 0;
        VariableType sum = _constant;
        foreach (var (index, scale) in _variables)
        {
            var scaledValue = scale * variables[index];
            sum += scaledValue;
            
            if (scaledValue.Size > maxSize)
                maxSize = scaledValue.Size;
        }

        if (0 < sum.Min)
            return RestrictResult.Infeasible;
        if (sum.Max <= 0)
            return RestrictResult.Complete;
        
        if (maxSize <= -sum.Min)
            return RestrictResult.NoChange;

        foreach(var (index, scale) in _variables)
        {
            if (scale == 0)
                continue;

            // note that minSum <= 0
            var elDiff = sum.Min / scale;
            
            // note that elDiff will be positive when scale is negative
            var elResult = scale > 0
                ? Variable.RestrictToMax(index, variables[index].Min - elDiff, variables)
                : Variable.RestrictToMin(index, variables[index].Max - elDiff, variables);
            
            if (elResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a higher maximum bound than the min possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }
        }

        if (sum.Min == 0)
            return RestrictResult.Complete;
        
        return RestrictResult.Change;
    }

    public override IEnumerable<int> GetVariableIndices() => _variables.Keys;

    public override IEnumerable<KeyValuePair<int, int>> GetVariables() => _variables;

    public override string ToString()
    {
        return String.Join(" + ", _variables.Select(pair => $"{pair.Value}*[{pair.Key}]")) + $" + {_constant}";
    }
}