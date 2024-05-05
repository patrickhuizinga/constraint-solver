namespace Solver.Lib;

public sealed class Add1Expression : Expression
{
    public override int Constant {get; }
    public int VariableIndex { get; }
    public int Scale { get; }

    public Add1Expression(int variableIndex)
    {
        VariableIndex = variableIndex;
        Scale = 1;
        Constant = 0;
    }

    public Add1Expression(Variable variable)
        :this(variable.Index)
    {
    }

    public Add1Expression(int variableIndex, int scale, int constant)
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
            
            case Add1Expression add1:
                if (add1.VariableIndex == VariableIndex)
                {
                    return new Add1Expression(
                        VariableIndex,
                        Scale + scale * add1.Scale,
                        Constant + scale * add1.Constant);
                }
                
                return new Add2Expression(
                    VariableIndex, Scale,
                    add1.VariableIndex, scale * add1.Scale,
                    Constant + scale * add1.Constant);
            
            case Add2Expression add2:
                if (VariableIndex == add2.FirstVariableIndex)
                {
                    return new Add2Expression(
                        VariableIndex, 
                        Scale + scale * add2.FirstScale,
                        add2.SecondVariableIndex,
                        add2.SecondScale,
                        Constant + scale * add2.Constant);
                }
                if (VariableIndex == add2.SecondVariableIndex)
                {
                    return new Add2Expression(
                        add2.FirstVariableIndex,
                        add2.FirstScale,
                        VariableIndex,
                        Scale + scale * add2.SecondScale,
                        Constant + scale * add2.Constant);
                }

                goto default;
            default:
                return new SumExpression(this, addition, scale);
        }
    }

    public override Expression Add(int addition)
    {
        if (addition == 0)
            return this;
        
        return new Add1Expression(VariableIndex, Scale, Constant + addition);
    }

    public override Expression Add(Variable addition, int scale)
    {
        if (scale == 0)
            return this;

        if (addition.Index == VariableIndex)
            return new Add1Expression(VariableIndex, Scale + 1, Constant);
        
        return new Add2Expression(
            VariableIndex, Scale,
            addition.Index, 1,
            Constant);
    }

    public override int GetMin(VariableCollection variables) =>
        Constant + variables[VariableIndex].GetMin(Scale);

    public override int GetMax(VariableCollection variables) =>
        Constant + variables[VariableIndex].GetMax(Scale);

    public override VariableType GetRange(VariableCollection variables)
    {
        return Constant + variables[VariableIndex] * Scale;
    }

    public override RestrictResult RestrictToMaxZero(VariableCollection variables)
    {
        return Scale switch
        {
            > 0 => Variable.RestrictToMax(VariableIndex, -Constant / Scale, variables),
            < 0 => Variable.RestrictToMin(VariableIndex, -Constant / Scale, variables),
            _ => 0 < Constant ? RestrictResult.Infeasible : RestrictResult.NoChange
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
}