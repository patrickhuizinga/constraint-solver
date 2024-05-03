namespace Solver.Lib;

public readonly record struct VariableType(int Min, int Max)
{
    public static readonly VariableType Zero = new(0, 0);
    public static readonly VariableType One = new(1, 1);
    public static readonly VariableType True = One;
    public static readonly VariableType False = Zero;

    public static VariableType Binary => new(0, 1);

    public static VariableType Constant(int value) => new(value, value);
    
    public static VariableType Constant(bool value) => value ? True : False;

    public static VariableType Range(int min, int max) => new(min, max);

    public static VariableType Range(Range range) => new(range.Start.Value, range.End.Value);


    public bool IsConstant => Min == Max;

    public bool TryGetConstant(out int value)
    {
        value = Min;
        return value == Max;
    }

    public int GetMin(int scale)
    {
        return scale > 0 ? scale * Min : scale * Max;
    }

    public int GetMax(int scale)
    {
        return scale > 0 ? scale * Max : scale * Min;
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