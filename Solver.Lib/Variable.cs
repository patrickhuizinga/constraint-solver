namespace Solver.Lib;

public abstract class Variable : IExpression
{
    public abstract int Min { get; }
    public abstract int Max { get; }
    public abstract int GetMax(Dictionary<Variable, Variable> variables);
    public abstract int GetMin(Dictionary<Variable, Variable> variables);
    public abstract Variable? TryRestrictToMin(int minValue);
    public abstract Variable? TryRestrictToMax(int maxValue);
    public abstract RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables);
    public virtual IEnumerable<Variable> GetVariables() => new[] { this };

    public abstract RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables);
    
    public static implicit operator Variable(int value)
    {
        return new ConstantVariable(value);
    }
}