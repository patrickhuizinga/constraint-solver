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
    public static readonly Expression Zero = new ConstantExpression(0);
    public static readonly Expression One = new ConstantExpression(1);
    public static readonly Expression True = One;
    public static readonly Expression False = Zero;
    
    public abstract int Constant { get; }
    
    public abstract VariableType GetRange(VariableCollection variables);

    public abstract RestrictResult RestrictToEqualZero(VariableCollection variables);
    
    public abstract RestrictResult RestrictToMaxZero(VariableCollection variables);

    public abstract Expression EliminateConstants(VariableCollection variables);

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
        return new VariableExpression(left).Add(right, -1);
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

    public void Reduce(ref Expression target, int variableIndex)
    {
        var thisScale = GetScale(variableIndex);
        if (thisScale == 0)
            throw new ArgumentException("This expression does not contain the variable", nameof(variableIndex));
        
        var targetScale = target.GetScale(variableIndex);

        // todo: optimize when one scale is a multiple of another
        while (targetScale != 0)
        {
            if (thisScale <= targetScale)
            {
                target -= this;
                targetScale -= thisScale;
            }
            else
            {
                target = this - target;
                targetScale = thisScale - targetScale;
            }
        }
    }

    public abstract int GetScale(int variableIndex);

    public static IConstraint operator <=(Expression left, Expression right)
    {
        return new LessThanConstraint(left.Add(right, -1));
    }

    public static IConstraint operator >=(Expression left, Expression right)
    {
        return new LessThanConstraint(right.Add(left, -1));
    }

    public static IConstraint operator <(Expression left, Expression right)
    {
        if (left is ConstantExpression ce)
            left = ce.Add(1);
        else
            right = right.Add(-1);
        
        return new LessThanConstraint(left.Add(right, -1));
    }

    public static IConstraint operator >(Expression left, Expression right)
    {
        if (left is ConstantExpression ce)
            left = ce.Add(-1);
        else
            right = right.Add(1);

        return new LessThanConstraint(right.Add(left, -1));
    }

    public static IConstraint operator ==(Expression left, Expression right)
    {
        return new EqualityConstraint(left.Add(right, -1));
    }

    public static IConstraint operator !=(Expression left, Expression right)
    {
        return new NotEqualConstraint(left.Add(right, -1));
    }

    public static IConstraint operator ==(Expression left, int right)
    {
        return new EqualityConstraint(left.Add(-right));
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
        return new VariableExpression(variable);
    }
}