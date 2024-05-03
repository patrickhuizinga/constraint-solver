using System.Numerics;

namespace Solver.Lib;

public abstract class Expression :
    IAdditionOperators<Expression, Expression, Expression>,
    IAdditionOperators<Expression, int, Expression>,
    ISubtractionOperators<Expression, int, Expression>,
    IComparisonOperators<Expression, Expression, IConstraint>
{
    public abstract int GetMin(IList<VariableType> variables);
    public abstract int GetMax(IList<VariableType> variables);
    public abstract RestrictResult RestrictToMin(int minValue, IList<VariableType> variables);
    public abstract RestrictResult RestrictToMax(int maxValue, IList<VariableType> variables);

    public abstract RestrictResult Exclude(int value, IList<VariableType> variables);

    public abstract IEnumerable<int> GetVariableIndices();

    public static Expression operator +(Expression left, Expression right)
    {
        return left.Add(right);
    }

    public static Expression operator +(Expression left, int right)
    {
        return left.Add(right);
    }

    public static Expression operator -(Expression left, int right)
    {
        return left.Add(-right);
    }

    public virtual Expression Add(Expression addition)
    {
        if (addition is SumExpression sum)
            return sum.Add(this);
        if (addition is Add2Expression add)
            return add.Add(this);

        return new Add2Expression(this, addition);
    }

    public virtual Expression Add(int addition)
    {
        return new Add2Expression(this, new ConstantExpression(addition));
    }

    public static IConstraint operator <=(Expression left, Expression right)
    {
        return ComparisonConstraint.Create(left, Comparison.LessEqual, right);
    }

    public static IConstraint operator >=(Expression left, Expression right)
    {
        return ComparisonConstraint.Create(left, Comparison.GreaterEqual, right);
    }

    public static IConstraint operator <(Expression left, Expression right)
    {
        if (right is ConstantExpression cr)
            right = cr - 1;
        else if (left is ConstantExpression cl)
            left = cl + 1;
        else if (right is SumExpression se)
            right = se.Add(-1);
        else
            left += 1;
        
        return left <= right;
    }

    public static IConstraint operator >(Expression left, Expression right)
    {
        if (right is ConstantExpression cr)
            right = cr + 1;
        else if (left is ConstantExpression cl)
            left = cl - 1;
        else if (right is SumExpression se)
            right = se.Add(1);
        else
            left -= 1;

        return left >= right;
    }

    public static IConstraint operator ==(Expression left, Expression right)
    {
        return new EqualityConstraint(left, right);
    }

    public static IConstraint operator !=(Expression left, Expression right)
    {
        return ComparisonConstraint.Create(left, Comparison.NotEquals, right);
    }

    public static IConstraint operator ==(Expression left, int right)
    {
        return new EqualityConstraint(left, right);
    }

    public static IConstraint operator !=(Expression left, int right)
    {
        return ComparisonConstraint.Create(left, Comparison.NotEquals, right);
    }

    public static implicit operator Expression(int value)
    {
        return new ConstantExpression(value);
    }
}