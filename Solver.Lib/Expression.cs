using System.Numerics;

namespace Solver.Lib;

public abstract class Expression :
    IAdditionOperators<Expression, Expression, Expression>,
    IAdditionOperators<Expression, int, Expression>,
    ISubtractionOperators<Expression, int, Expression>,
    IComparisonOperators<Expression, Expression, EqualityConstraint>
{
    public abstract int GetMin(Dictionary<Variable, Variable> variables);
    public abstract int GetMax(Dictionary<Variable, Variable> variables);
    public abstract RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables);
    public abstract RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables);

    public abstract RestrictResult Exclude(int value, Dictionary<Variable, Variable> variables);

    public abstract IEnumerable<Variable> GetVariables();

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
        return new Add2Expression(this, new ConstantVariable(addition));
    }

    public static EqualityConstraint operator <=(Expression left, Expression right)
    {
        return new EqualityConstraint(left, Comparison.LessEqual, right);
    }

    public static EqualityConstraint operator >=(Expression left, Expression right)
    {
        return new EqualityConstraint(left, Comparison.GreaterEqual, right);
    }

    public static EqualityConstraint operator <(Expression left, Expression right)
    {
        if (right is ConstantVariable cr)
            right = cr.Add(-1);
        else if (left is ConstantVariable cl)
            left = cl.Add(1);
        else if (right is SumExpression se)
            right = se.Add(-1);
        else
            left += 1;
        
        return left <= right;
    }

    public static EqualityConstraint operator >(Expression left, Expression right)
    {
        if (right is ConstantVariable cr)
            right = cr.Add(1);
        else if (left is ConstantVariable cl)
            left = cl.Add(-1);
        else if (right is SumExpression se)
            right = se.Add(1);
        else
            left -= 1;

        return left >= right;
    }

    public static EqualityConstraint operator ==(Expression left, Expression right)
    {
        return new EqualityConstraint(left, Comparison.Equals, right);
    }

    public static EqualityConstraint operator !=(Expression left, Expression right)
    {
        return new EqualityConstraint(left, Comparison.NotEquals, right);
    }

    public static implicit operator Expression(int value)
    {
        return new ConstantVariable(value);
    }
}