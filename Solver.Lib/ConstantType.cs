namespace Solver.Lib;

public class ConstantType(int value) : VariableType
{
    public ConstantType(bool value)
        : this(value ? 1 : 0)
    {
    }
    
    public ConstantType Add(int addition)
    {
        return new ConstantType(Value + addition);
    }

    public int Value { get; } = value;

    public override int Min => Value;

    public override int Max => Value;
    
    public override VariableType? TryRestrictToMin(int minValue)
    {
        return minValue <= Value
            ? this
            : null;
    }
    
    public override VariableType? TryRestrictToMax(int maxValue)
    {
        return Value <= maxValue
            ? this
            : null;
    }

    public override VariableType? TryExclude(int value)
    {
        return Value != value
            ? this
            : null;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator ConstantType(int value)
    {
        return new ConstantType(value);
    }

    public static implicit operator ConstantType(bool value)
    {
        return new ConstantType(value ? 1 : 0);
    }
}