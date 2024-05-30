namespace Solver.Lib;

public sealed class VariableExpression : Expression
{
    public override int Constant { get; }
    public int VariableIndex { get; }
    public int Scale { get; }

    public VariableExpression(Variable variable)
        : this(variable.Index, 1, 0)
    {
    }

    public VariableExpression(int variableIndex, int scale, int constant)
    {
        VariableIndex = variableIndex;
        Scale = scale;
        Constant = constant;
    }

    public override Expression Add(Expression addition, int scale)
    {
        if (scale == 0)
            return this;
        
        switch (addition)
        {
            case ConstantExpression cv:
                return Add(scale * cv.Value);
            
            case VariableExpression add1:
                if (add1.VariableIndex == VariableIndex)
                {
                    return new VariableExpression(
                        VariableIndex,
                        Scale + scale * add1.Scale,
                        Constant + scale * add1.Constant);
                }
                
                return new SumExpression(this, addition, scale);
            default:
                return new SumExpression(this, addition, scale);
        }
    }

    public override Expression Add(int addition)
    {
        if (addition == 0)
            return this;
        
        return new VariableExpression(VariableIndex, Scale, Constant + addition);
    }

    public override Expression Add(Variable addition, int scale)
    {
        if (scale == 0)
            return this;

        if (addition.Index != VariableIndex)
            return new SumExpression(this, addition, scale);
        
        if (Scale + scale == 0)
            return new ConstantExpression(Constant);
            
        return new VariableExpression(VariableIndex, Scale + scale, Constant);

    }

    public override int GetScale(int variableIndex) => variableIndex == VariableIndex ? Scale : 0;
    
    public override Expression EliminateConstants(VariableCollection variables)
    {
        if (variables[VariableIndex].TryGetConstant(out var value))
            return new ConstantExpression(Constant + Scale * value);

        return this;
    }

    public override VariableType GetRange(VariableCollection variables)
    {
        return Constant + variables[VariableIndex] * Scale;
    }

    public override RestrictResult RestrictToEqualZero(VariableCollection variables)
    {
        if (Scale == 0)
            return Constant == 0 ? RestrictResult.Complete : RestrictResult.Infeasible;

        if (Constant % Scale != 0)
            return RestrictResult.Infeasible;

        var value = -Constant / Scale;
        
        var variable = variables[VariableIndex];
        if (variable.TryGetConstant(out var constant))
            return constant == value ? RestrictResult.Complete : RestrictResult.Infeasible;

        if (variable.Contains(value))
        {
            variables[VariableIndex] = value;
            return RestrictResult.Complete;
        }

        return RestrictResult.Infeasible;
    }

    public override RestrictResult RestrictToMaxZero(VariableCollection variables)
    {
        return Scale switch
        {
            > 0 => Variable.RestrictToMax(VariableIndex, -Constant / Scale, variables),
            < 0 => Variable.RestrictToMin(VariableIndex, -Constant / Scale, variables),
            _ => 0 < Constant ? RestrictResult.Infeasible : RestrictResult.Complete
        };
    }

    public override IEnumerable<int> GetVariableIndices()
    {
        yield return VariableIndex;
    }

    public override IEnumerable<KeyValuePair<int, int>> GetVariables()
    {
        yield return KeyValuePair.Create(VariableIndex, Scale);
    }

    public override string ToString()
    {
        return $"{Scale}*[{VariableIndex}] + {Constant}";
    }
}