namespace Solver.Lib;

public class RangeType : VariableType
{
    public RangeType(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public static VariableType Create(int min, int max)
    {
        if (min == 0 && max == 1)
            return new BinaryType();
        if (min == max)
            return new ConstantType(min);

        return new RangeType(min, max);
    }

    public override int Min { get; }

    public override int Max { get; }

    public override VariableType? TryRestrictToMin(int minValue)
    {
        if (minValue <= Min)
            return this;
        if (minValue <= Max)
            return Create(minValue, Max);

        return null;
    }

    public override VariableType? TryRestrictToMax(int maxValue)
    {
        if (Max <= maxValue)
            return this;
        if (Min <= maxValue)
            return Create(Min, maxValue);

        return null;
    }

    public override VariableType TryExclude(int value)
    {
        if (value == Min)
            return Create(value + 1, Max);

        if (value == Max)
            return Create(Min, value - 1);

        return this;    
    }

    public override string ToString()
    {
        return $"[{Min}-{Max}]";
    }
}