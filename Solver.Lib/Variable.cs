namespace Solver.Lib;

public abstract class Variable : Expression
{
    public abstract int Min { get; }
    public abstract int Max { get; }
    public abstract Variable? TryRestrictToMin(int minValue);
    public abstract Variable? TryRestrictToMax(int maxValue);
    public abstract Variable? TryExclude(int value);
    public override IEnumerable<Variable> GetVariables() => new[] { this };

    public static implicit operator Variable(int value)
    {
        return new ConstantVariable(value);
    }
}