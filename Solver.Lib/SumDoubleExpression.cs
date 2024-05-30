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

    public SumDoubleExpression(IEnumerable<Variable> variables, IEnumerable<double> scales)
        :this(variables.Zip(scales))
    {
    }

    public SumDoubleExpression(IEnumerable<(Variable, double scale)> variables)
    {
        _variables = new SortedList<int, double>();
        
        foreach (var (variable, scale) in variables) 
            AddVariable(variable.Index, scale);
    }

    public SumDoubleExpression(params Expression[] expressions)
    {
        _variables = new SortedList<int, double>();
        foreach (var v in expressions) 
            AddVariables(v, 1, ref _constant);
    }

    public SumDoubleExpression(IEnumerable<Expression> expressions, IEnumerable<double> scales)
        :this(expressions.Zip(scales))
    {
    }

    public SumDoubleExpression(IEnumerable<(Expression, double scale)> expressions)
    {
        _variables = new SortedList<int, double>();
        
        foreach (var (expression, scale) in expressions) 
            AddVariables(expression, scale, ref _constant);
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
        {
            var oldValue = -_variables.GetValueAtIndex(i);
            _variables.SetValueAtIndex(i, oldValue + scale);
        }
    }

    public static SumDoubleExpression Create(params Variable[] variables)
    {
        return new SumDoubleExpression(variables);
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
        return Constant + _variables.Sum(variables.GetMin);
    }

    public override double GetMax(VariableCollection variables)
    {
        return Constant + _variables.Sum(variables.GetMax);
    }

    public override double GetScale(int variableIndex)
    {
        return _variables.GetValueOrDefault(variableIndex);
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