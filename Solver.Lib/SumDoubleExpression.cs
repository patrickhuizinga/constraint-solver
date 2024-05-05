namespace Solver.Lib;

public sealed class SumDoubleExpression : DoubleExpression
{
    private readonly SortedList<int, double> _variables;
    private readonly double _constant;

    public override double Constant => _constant;

    public SumDoubleExpression(double constant)
    {
        _variables = new SortedList<int, double>();
        _constant = constant;
    }

    public SumDoubleExpression(Expression expression, double scale)
    {
        _variables = new SortedList<int, double>();
        AddVariables(expression, scale, ref _constant);
    }

    public SumDoubleExpression(Variable variable, double scale)
    {
        _variables = new SortedList<int, double> { { variable.Index, scale } };
    }

    public SumDoubleExpression(params Variable[] variables)
    {
        _variables = new SortedList<int, double>();
        foreach (var v in variables) 
            AddVariable(v.Index, 1);
    }

    private SumDoubleExpression(SumDoubleExpression source, double constant)
    {
        _variables = new SortedList<int, double>(source._variables);
        _constant = source._constant + constant;
    }

    private SumDoubleExpression(SumDoubleExpression source, DoubleExpression addition, double scale)
    {
        _variables = new SortedList<int, double>(source._variables);
        _constant = source._constant;
        
        AddVariables(addition, scale, ref _constant);
    }

    private SumDoubleExpression(SumDoubleExpression source, Variable addition, double scale)
    {
        _variables = new SortedList<int, double>(source._variables);
        _constant = source._constant;
        
        AddVariable(addition.Index, scale);
    }

    private void AddVariables(DoubleExpression expression, double scale, ref double constant)
    {
        constant += scale * expression.Constant;
        
        foreach (var (i, s) in expression.GetVariables()) 
            AddVariable(i, scale * s);
    }

    private void AddVariables(Expression expression, double scale, ref double constant)
    {
        constant += scale * expression.Constant;
        
        foreach (var (i, s) in expression.GetVariables()) 
            AddVariable(i, scale * s);
    }

    private void AddVariable(int index, double scale)
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

    public SumDoubleExpression Add(Expression addition, int scale)
    {
        if (scale == 0)
            return this;

        if (addition is ConstantExpression cv)
            return Add(scale * cv.Value);
        
        return new SumDoubleExpression(this, addition, scale);
    }

    public override SumDoubleExpression Add(DoubleExpression addition, double scale)
    {
        if (scale == 0)
            return this;

        return new SumDoubleExpression(this, addition, scale);
    }

    public override SumDoubleExpression Add(double addition)
    {
        if (addition == 0)
            return this;
        
        return new SumDoubleExpression(this, addition);
    }

    public override SumDoubleExpression Add(Variable addition, double scale)
    {
        if (scale == 0)
            return this;

        return new SumDoubleExpression(this, addition, scale);
    }

    public override double GetMin(VariableCollection variables)
    {
        return Constant + _variables.Sum(pair => variables[pair.Key].GetMin(pair.Value));
    }

    public override IEnumerable<int> GetVariableIndices()
    {
        return _variables.Keys;
    }

    public override IEnumerable<KeyValuePair<int, double>> GetVariables()
    {
        return _variables;
    }
}