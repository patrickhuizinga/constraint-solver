namespace Solver.Lib;

public class ConstantExpression(int value) : Expression
{
    public override Expression Add(Expression addition, int scale)
    {
        if (scale == 0)
            return this;
        
        if (Value == 0 && scale == 1)
            return addition;
        
        switch (addition)
        {
            case ConstantExpression cv:
                return Add(scale * cv.Constant);
            case Add1Expression add1:
                return new Add1Expression(
                    add1.VariableIndex, scale * add1.Scale, 
                    add1.Constant + Value);
            case Add2Expression add2:
                return new Add2Expression(
                    add2.FirstVariableIndex, scale * add2.FirstScale,
                    add2.SecondVariableIndex, scale * add2.SecondScale,
                    add2.Constant + Value);
            default:
                return new SumExpression(this, addition, scale);
        }
    }

    public override Expression Add(int addition)
    {
        if (addition == 0)
            return this;
        
        return new ConstantExpression(Value + addition);
    }

    public override Expression Add(Variable addition, int scale)
    {
        if (scale == 0)
            return this;
        
        return new Add1Expression(addition.Index, scale, Value);
    }

    public int Value { get; } = value;

    public override int Constant => Value;

    public override int GetMin(VariableCollection variables) => Value;

    public override int GetMax(VariableCollection variables) => Value;
    
    public override RestrictResult RestrictToMaxZero(VariableCollection variables)
    {
        return Value <= 0
            ? RestrictResult.NoChange
            : RestrictResult.Infeasible;
    }

    public override IEnumerable<int> GetVariableIndices() => Enumerable.Empty<int>();
    
    public override IEnumerable<KeyValuePair<int, int>> GetVariables() => Enumerable.Empty<KeyValuePair<int, int>>();

    public override string ToString()
    {
        return Value.ToString();
    }

    public static Expression operator +(ConstantExpression left, int right)
    {
        return new ConstantExpression(left.Value + right);
    }

    public static Expression operator -(ConstantExpression left, int right)
    {
        return new ConstantExpression(left.Value - right);
    }
}