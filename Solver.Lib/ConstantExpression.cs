namespace Solver.Lib;

public class ConstantExpression(int value) : Expression
{
    public override Expression Add(int addition)
    {
        return new ConstantExpression(Value + addition);
    }

    public int Value { get; } = value;

    public override int GetMin(IList<VariableType> variables) => Value;

    public override int GetMax(IList<VariableType> variables) => Value;
    
    public override RestrictResult RestrictToMin(int minValue, IList<VariableType> variables)
    {
        return minValue <= Value
            ? RestrictResult.NoChange
            : RestrictResult.Infeasible;
    }
    
    public override RestrictResult RestrictToMax(int maxValue, IList<VariableType> variables)
    {
        return Value <= maxValue
            ? RestrictResult.NoChange
            : RestrictResult.Infeasible;
    }

    public override RestrictResult Exclude(int value, IList<VariableType> variables)
    {
        return value == Value
            ? RestrictResult.Infeasible
            : RestrictResult.NoChange;
    }

    public override IEnumerable<int> GetVariableIndices() => Enumerable.Empty<int>();

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