namespace Solver.Lib;

public abstract class VariableType
{
    public abstract int Min { get; }
    public abstract int Max { get; }

    public abstract VariableType? TryRestrictToMin(int minValue);
    public abstract VariableType? TryRestrictToMax(int maxValue);
    public abstract VariableType? TryExclude(int value);

    
    public static implicit operator VariableType(int value)
    {
        return new ConstantType(value);
    }
    
    public static implicit operator VariableType(bool value)
    {
        return new ConstantType(value ? 1 : 0);
    }
    
    public static implicit operator VariableType(Range range)
    {
        var min = range.Start.Value;
        var max = range.End.Value;

        return new RangeType(min, max);
    }
}