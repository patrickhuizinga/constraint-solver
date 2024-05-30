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


    public int Size => Max - Min;
    
    public bool IsConstant => Min == Max;

    public bool TryGetConstant(out int value)
    {
        value = Min;
        return value == Max;
    }

    public bool Contains(int value) => Min <= value && value <= Max;

    public int GetMin(int scale)
    {
        return scale > 0 ? scale * Min : scale * Max;
    }

    public double GetMin(double scale)
    {
        return scale > 0 ? scale * Min : scale * Max;
    }

    public int GetMax(int scale)
    {
        return scale > 0 ? scale * Max : scale * Min;
    }

    public double GetMax(double scale)
    {
        return scale > 0 ? scale * Max : scale * Min;
    }

    public override string ToString()
    {
        if (Min == Max)
            return Min.ToString();
        
        return $"[{Min}..{Max}]";
    }

    
    public static implicit operator VariableType(int value) => new(value, value);

    public static implicit operator VariableType(bool value) => value ? True : False;

    public static implicit operator VariableType(Range range) => new(range.Start.Value, range.End.Value);

    public static VariableType operator +(VariableType left, VariableType right)
    {
        return new VariableType(left.Min + right.Min, left.Max + right.Max);
    }

    public static VariableType operator +(int left, VariableType right)
    {
        return new VariableType(left + right.Min, left + right.Max);
    }

    public static VariableType operator +(VariableType left, int right)
    {
        return new VariableType(left.Min + right, left.Max + right);
    }

    public static VariableType operator *(VariableType left, int scale)
    {
        return scale > 0
            ? new VariableType(scale * left.Min, scale * left.Max)
            : new VariableType(scale * left.Max, scale * left.Min);
    }

    public static VariableType operator *(int scale, VariableType right)
    {
        return scale > 0
            ? new VariableType(scale * right.Min, scale * right.Max)
            : new VariableType(scale * right.Max, scale * right.Min);
    }


}