namespace Solver.Lib;

public class RangeVariable : Variable
{
    public RangeVariable(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public static Variable Create(int min, int max)
    {
        if (min == 0 && max == 1)
            return new BinaryVariable();
        if (min == max)
            return new ConstantVariable(min);

        return new RangeVariable(min, max);
    }

    public override int Min { get; }

    public override int Max { get; }

    public override int GetMin(Dictionary<Variable, Variable> variables) => variables[this].Min;

    public override int GetMax(Dictionary<Variable, Variable> variables) => variables[this].Max;

    public override Variable? TryRestrictToMin(int minValue)
    {
        if (minValue <= Min)
            return this;
        if (minValue <= Max)
            return Create(minValue, Max);

        return null;
    }

    public override Variable? TryRestrictToMax(int maxValue)
    {
        if (Max <= maxValue)
            return this;
        if (Min <= maxValue)
            return Create(Min, maxValue);

        return null;
    }

    public override Variable TryExclude(int value)
    {
        if (value == Min)
            return Create(value + 1, Max);

        if (value == Max)
            return Create(Min, value - 1);

        if (value < Min || Max < value)
            return this;

        return CompoundVariable.Create(
            Create(Min, value - 1),
            Create(value + 1, Max));
    }

    public override string ToString()
    {
        return $"[{Min}-{Max}]";
    }
}