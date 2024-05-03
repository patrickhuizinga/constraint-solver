namespace Solver.Lib;

// == and != operators are *not* boolean, therefor overriding Equals and GetHashcode isn't needed
#pragma warning disable CS0660, CS0661
public readonly struct Variable(int index)
{
    public int Index { get; } = index;

    public static RestrictResult RestrictToMin(int index, int minValue, IList<VariableType> variables)
    {
        var oldVal = variables[index];

        if (minValue <= oldVal.Min)
        {
            return RestrictResult.NoChange;
        }
        
        if (minValue <= oldVal.Max)
        {
            variables[index] = oldVal with { Min = minValue };
            return RestrictResult.Change;
        }

        return RestrictResult.Infeasible;
    }

    public static RestrictResult RestrictToMax(int index, int maxValue, IList<VariableType> variables)
    {
        var oldVal = variables[index];
        if (oldVal.Max <= maxValue)
        {
            return RestrictResult.NoChange;
        }

        if (oldVal.Min <= maxValue)
        {
            variables[index] = oldVal with { Max = maxValue };
            return RestrictResult.Change;
        }

        return RestrictResult.Infeasible;
    }

    public static RestrictResult Exclude(int index, int value, IList<VariableType> variables)
    {
        var oldVal = variables[index];
        
        if (oldVal.TryGetConstant(out int val))
            return val == value ? RestrictResult.Infeasible : RestrictResult.NoChange;
        
        if (value == oldVal.Min)
        {   
            variables[index] = oldVal with { Min = value + 1 };
            return RestrictResult.Change;
        }

        if (value == oldVal.Max)
        {
            variables[index] = oldVal with { Max = value - 1 };
            return RestrictResult.Change;
        }

        return RestrictResult.NoChange;
    }

    public static Expression operator +(Variable left, Variable right)
    {
        return SumExpression.Create(left, right);
    }

    public static Expression operator +(Variable left, int right)
    {
        return new Add1Expression(left.Index, 1, right);
    }

    public static Expression operator -(Variable left, int right)
    {
        return new Add1Expression(left.Index, 1, -right);
    }

    public static IConstraint operator <=(Variable left, Variable right)
    {
        return ComparisonConstraint.Create(left, Comparison.LessEqual, right);
    }

    public static IConstraint operator >=(Variable left, Variable right)
    {
        return ComparisonConstraint.Create(left, Comparison.GreaterEqual, right);
    }

    public static IConstraint operator ==(Variable left, Variable right)
    {
        return new EqualityConstraint(left, right);
    }

    public static IConstraint operator !=(Variable left, Variable right)
    {
        return ComparisonConstraint.Create(left, Comparison.NotEquals, right);
    }

    public static IConstraint operator ==(Variable left, int right)
    {
        return new EqualityConstraint(left, right);
    }

    public static IConstraint operator !=(Variable left, int right)
    {
        return ComparisonConstraint.Create(left, Comparison.NotEquals, right);
    }
}