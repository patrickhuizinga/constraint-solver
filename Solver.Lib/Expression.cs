using System.Numerics;

namespace Solver.Lib;

public abstract class Expression :
    IAdditionOperators<Expression, Expression, SumExpression>
{
    public abstract int GetMin(Dictionary<Variable, Variable> variables);
    public abstract int GetMax(Dictionary<Variable, Variable> variables);
    public abstract RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables);
    public abstract RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables);

    public abstract RestrictResult Exclude(int value, Dictionary<Variable, Variable> variables);
    
    public abstract IEnumerable<Variable> GetVariables();
    
    public static SumExpression operator +(Expression left, Expression right)
    {
        return left.Add(right);
    }

    public virtual SumExpression Add(Expression addition)
    {
        if (addition is SumExpression sum)
            return sum.Add(this);
        
        return new SumExpression(this, addition);
    }

    public static Constraint operator <=(Expression left, Expression right)
    {
        return new Constraint(left, Comparison.LessThan, right);
    }

    public static Constraint operator >=(Expression left, Expression right)
    {
        return new Constraint(left, Comparison.GreaterThan, right);
    }
    
    public static Constraint operator ==(Expression left, Expression right)
    {
        return new Constraint(left, Comparison.Equals, right);
    }

    public static Constraint operator !=(Expression left, Expression right)
    {
        return new Constraint(left, Comparison.NotEquals, right);
    }

    public static implicit operator Expression(int value)
    {
        return new ConstantVariable(value);
    }
}