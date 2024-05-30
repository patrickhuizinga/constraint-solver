namespace Solver.Lib;

// == and != operators are *not* boolean, therefor overriding Equals and GetHashcode isn't needed
#pragma warning disable CS0660, CS0661
public readonly struct Variable(int index)
{
    public static readonly Variable None = new(0);
    public int Index { get; } = index;

    public static RestrictResult RestrictToMin(int index, int minValue, VariableCollection variables)
    {
        var oldVal = variables[index];

        if (minValue < oldVal.Min)
        {
            return RestrictResult.NoChange;
        }
        if (minValue == oldVal.Min)
        {
            return minValue == oldVal.Max ? RestrictResult.Complete : RestrictResult.NoChange;
        }
        
        if (minValue <= oldVal.Max)
        {
            variables[index] = oldVal with { Min = minValue };
            return minValue == oldVal.Max ? RestrictResult.Complete : RestrictResult.Change;
        }

        return RestrictResult.Infeasible;
    }

    public static RestrictResult RestrictToMax(int index, int maxValue, VariableCollection variables)
    {
        var oldVal = variables[index];
        if (oldVal.Max < maxValue)
        {
            return RestrictResult.NoChange;
        }
        if (oldVal.Max == maxValue)
        {
            return oldVal.Min == maxValue ? RestrictResult.Complete : RestrictResult.NoChange;
        }

        if (oldVal.Min <= maxValue)
        {
            variables[index] = oldVal with { Max = maxValue };
            return oldVal.Min == maxValue ? RestrictResult.Complete : RestrictResult.Change;
        }

        return RestrictResult.Infeasible;
    }

    public static RestrictResult Exclude(int index, int value, VariableCollection variables)
    {
        var oldVal = variables[index];
        
        if (oldVal.TryGetConstant(out int val))
            return val == value ? RestrictResult.Infeasible : RestrictResult.Complete;
        
        if (value == oldVal.Min)
        {   
            variables[index] = oldVal with { Min = value + 1 };
            return oldVal.Size == 1 ? RestrictResult.Complete : RestrictResult.Change;
        }

        if (value == oldVal.Max)
        {
            variables[index] = oldVal with { Max = value - 1 };
            return oldVal.Size == 1 ? RestrictResult.Complete : RestrictResult.Change;
        }

        return RestrictResult.NoChange;
    }

    public static Expression operator +(Variable left, Variable right)
    {
        return SumExpression.Create(left, right);
    }

    public static Expression operator +(Variable left, int right)
    {
        return new VariableExpression(left.Index, 1, right);
    }

    public static Expression operator +(int left, Variable right)
    {
        return new VariableExpression(right.Index, 1, left);
    }

    public static Expression operator -(Variable left, int right)
    {
        return new VariableExpression(left.Index, 1, -right);
    }

    public static Expression operator -(int left, Variable right)
    {
        return new VariableExpression(right.Index, -1, left);
    }

    public static Expression operator -(Variable left, Variable right)
    {
        if (left.Index == right.Index)
            return Expression.Zero;

        return new SumExpression([left, right], [1, -1], 0);
    }

    public static Expression operator -(Variable variable)
    {
        return new VariableExpression(variable.Index, -1, 0);
    }

    public static Expression operator *(Variable variable, int scale)
    {
        if (scale == 0)
            return Expression.Zero;

        return new VariableExpression(variable.Index, scale, 0);
    }

    public static Expression operator *(int scale, Variable variable)
    {
        if (scale == 0)
            return Expression.Zero;

        return new VariableExpression(variable.Index, scale, 0);
    }

    public static DoubleExpression operator *(Variable variable, double scale)
    {
        if (scale == 0)
            return DoubleExpression.Zero;

        return new SumDoubleExpression(variable, scale);
    }

    public static DoubleExpression operator *(double scale, Variable variable)
    {
        if (scale == 0)
            return DoubleExpression.Zero;

        return new SumDoubleExpression(variable, scale);
    }

    public static IConstraint operator <=(Variable left, Variable right)
    {
        return new LessThanConstraint(left - right);
    }

    public static IConstraint operator >=(Variable left, Variable right)
    {
        return new LessThanConstraint(right - left);
    }

    public static IConstraint operator ==(Variable left, Variable right)
    {
        return new EqualityConstraint(left - right);
    }

    public static IConstraint operator !=(Variable left, Variable right)
    {
        return new NotEqualConstraint(left - right);
    }

    public static IConstraint operator ==(Variable left, int right)
    {
        return new EqualityConstraint(left - right);
    }

    public static IConstraint operator !=(Variable left, int right)
    {
        return new NotEqualConstraint(left - right);
    }

    public override string ToString()
    {
        return $"[{Index}]";
    }
}