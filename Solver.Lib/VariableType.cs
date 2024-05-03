namespace Solver.Lib;

public record VariableType(int Min, int Max)
{
    public static readonly VariableType Zero = new(0, 0);
    public static readonly VariableType One = new(1, 1);
    public static readonly VariableType True = One;
    public static readonly VariableType False = Zero;

    public static VariableType Binary => new(0, 1);

    public static VariableType Constant(int value) => new(value, value);
    
    public static VariableType Constant(bool value) => value ? True : False;


    public bool IsConstant => Min == Max;

    public bool TryGetConstant(out int value)
    {
        value = Min;
        return value == Max;
    }

    public VariableType? TryRestrictToMin(int minValue)
    {
        if (minValue <= Min)
            return this;
        if (minValue <= Max)
            return this with { Min = minValue };

        return null;
    }

    public VariableType? TryRestrictToMax(int maxValue)
    {
        if (Max <= maxValue)
            return this;
        if (Min <= maxValue)
            return this with { Max = maxValue };

        return null;
    }

    public VariableType TryExclude(int value)
    {
        if (value == Min)
            return this with { Min = value + 1 };

        if (value == Max)
            return this with { Max = value - 1 };

        return this;    
    }

    public override string ToString()
    {
        if (Min == Max)
            return Min.ToString();
        
        return $"[{Min}-{Max}]";
    }

    
    public static implicit operator VariableType(int value) => new(value, value);

    public static implicit operator VariableType(bool value) => value ? True : False;

    public static implicit operator VariableType(Range range) => new(range.Start.Value, range.End.Value);
}