using System.Numerics;

namespace Solver.Lib;

// == and != operators are *not* boolean, therefor overriding Equals and GetHashcode isn't needed
#pragma warning disable CS0660, CS0661 
public abstract class Expression :
    IAdditiveIdentity<Expression, Expression>,
    IAdditionOperators<Expression, Expression, Expression>,
    IAdditionOperators<Expression, int, Expression>,
    IAdditionOperators<Expression, Variable, Expression>,
    ISubtractionOperators<Expression, Expression, Expression>,
    ISubtractionOperators<Expression, int, Expression>,
    ISubtractionOperators<Expression, Variable, Expression>,
    IMultiplicativeIdentity<Expression, Expression>,
    IMultiplyOperators<Expression, int, Expression>,
    IComparisonOperators<Expression, Expression, IConstraint>,
    IEqualityOperators<Expression, int, IConstraint>
{
    public static readonly ConstantExpression Zero = new(0);
    public static readonly ConstantExpression One = new(1);
    public static readonly ConstantExpression True = One;
    public static readonly ConstantExpression False = Zero;
    
    public abstract int Constant { get; }
    
    public abstract int GetMin(VariableCollection variables);
    public abstract int GetMax(VariableCollection variables);

    public abstract VariableType GetRange(VariableCollection variables);
    
    public abstract RestrictResult RestrictToMaxZero(VariableCollection variables);

    public abstract IEnumerable<int> GetVariableIndices();
    public abstract IEnumerable<KeyValuePair<int, int>> GetVariables();

    public static Expression AdditiveIdentity => Zero;

    public static Expression operator +(Expression left, Expression right)
    {
        return left.Add(right, 1);
    }

    public static Expression operator +(Expression left, int right)
    {
        return left.Add(right);
    }

    public static Expression operator +(int left, Expression right)
    {
        return right.Add(left);
    }

    public static Expression operator +(Expression left, Variable right)
    {
        return left.Add(right, 1);
    }

    public static Expression operator +(Variable left, Expression right)
    {
        return right.Add(left, 1);
    }

    public static Expression operator -(Expression left, Expression right)
    {
        return left.Add(right, -1);
    }

    public static Expression operator -(Expression left, int right)
    {
        return left.Add(-right);
    }

    public static Expression operator -(int left, Expression right)
    {
        return new ConstantExpression(left).Add(right, -1);
    }

    public static Expression operator -(Expression left, Variable right)
    {
        return left.Add(right, -1);
    }

    public static Expression operator -(Variable left, Expression right)
    {
        return new Add1Expression(left).Add(right, -1);
    }

    public static Expression operator -(Expression expression)
    {
        return Zero.Add(expression, -1);
    }

    public abstract Expression Add(Expression addition, int scale);

    public abstract Expression Add(int addition);

    public abstract Expression Add(Variable addition, int scale);

    public static Expression MultiplicativeIdentity => One;

    public static Expression operator *(Expression expression, int scale)
    {
        return Zero.Add(expression, scale);
    }

    public static Expression operator *(int scale, Expression expression)
    {
        return Zero.Add(expression, scale);
    }

    public static IConstraint operator <=(Expression left, Expression right)
    {
        return new LessThanConstraint(left, right);
    }

    public static IConstraint operator >=(Expression left, Expression right)
    {
        return new LessThanConstraint(right, left);
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
        return new NotEqualConstraint(left, right);
    }

    public static IConstraint operator ==(Expression left, int right)
    {
        return new EqualityConstraint(left, right);
    }

    public static IConstraint operator !=(Expression left, int right)
    {
        return new NotEqualConstraint(left.Add(-right));
    }

    public static implicit operator Expression(int value)
    {
        return new ConstantExpression(value);
    }

    public static implicit operator Expression(Variable variable)
    {
        return new Add1Expression(variable);
    }
}