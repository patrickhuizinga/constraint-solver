using System.Numerics;

namespace Solver.Lib;

public abstract class DoubleExpression :
    IAdditiveIdentity<DoubleExpression, DoubleExpression>,
    IAdditionOperators<DoubleExpression, DoubleExpression, DoubleExpression>,
    IAdditionOperators<DoubleExpression, double, DoubleExpression>,
    IAdditionOperators<DoubleExpression, Variable, DoubleExpression>,
    ISubtractionOperators<DoubleExpression, DoubleExpression, DoubleExpression>,
    ISubtractionOperators<DoubleExpression, double, DoubleExpression>,
    ISubtractionOperators<DoubleExpression, Variable, DoubleExpression>,
    IMultiplicativeIdentity<DoubleExpression, DoubleExpression>,
    IMultiplyOperators<DoubleExpression, double, DoubleExpression>
{
    public static readonly DoubleExpression Zero = new SumDoubleExpression(0.0);
    public static readonly DoubleExpression One = new SumDoubleExpression(1.0);
    public abstract double Constant { get; }
    
    public abstract double GetMin(VariableCollection variables);

    public abstract double GetMax(VariableCollection variables);
    public abstract double GetScale(int variableIndex);

    public abstract IEnumerable<int> GetVariableIndices();
    public abstract IEnumerable<KeyValuePair<int, double>> GetVariables();

    public static DoubleExpression AdditiveIdentity => Zero;

    public static DoubleExpression operator +(DoubleExpression left, DoubleExpression right)
    {
        return left.Add(right, 1);
    }

    public static DoubleExpression operator +(DoubleExpression left, double right)
    {
        return left.Add(right);
    }

    public static DoubleExpression operator +(double left, DoubleExpression right)
    {
        return right.Add(left);
    }

    public static DoubleExpression operator +(DoubleExpression left, Variable right)
    {
        return left.Add(right, 1);
    }

    public static DoubleExpression operator +(Variable left, DoubleExpression right)
    {
        return right.Add(left, 1);
    }

    public static DoubleExpression operator -(DoubleExpression left, DoubleExpression right)
    {
        return left.Add(right, -1);
    }

    public static DoubleExpression operator -(DoubleExpression left, double right)
    {
        return left.Add(-right);
    }

    public static DoubleExpression operator -(double left, DoubleExpression right)
    {
        return new SumDoubleExpression(left) - right;
    }

    public static DoubleExpression operator -(DoubleExpression left, Variable right)
    {
        return left.Add(right, -1);
    }

    public static DoubleExpression operator -(Variable left, DoubleExpression right)
    {
        return new SumDoubleExpression(left).Add(right, -1);
    }

    public static DoubleExpression operator -(DoubleExpression expression)
    {
        return Zero.Add(expression, -1);
    }

    public abstract DoubleExpression Add(DoubleExpression addition, double scale);

    public abstract DoubleExpression Add(double addition);

    public abstract DoubleExpression Add(Variable addition, double scale);

    public static DoubleExpression MultiplicativeIdentity => One;

    public static DoubleExpression operator *(DoubleExpression expression, double scale)
    {
        return Zero.Add(expression, scale);
    }

    public static DoubleExpression operator *(double scale, DoubleExpression expression)
    {
        return Zero.Add(expression, scale);
    }

    public static implicit operator DoubleExpression(double value)
    {
        return new SumDoubleExpression(Expression.One, value);
    }

    public static implicit operator DoubleExpression(Expression expression)
    {
        return new SumDoubleExpression(expression, 1.0);
    }

    public static implicit operator DoubleExpression(Variable variable)
    {
        return new SumDoubleExpression(variable);
    }
}