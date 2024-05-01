namespace Solver.Lib;

public class ConstantVariable(int value) : Variable
{
    public int Value { get; } = value;

    public override int Min => Value;

    public override int Max => Value;

    public override int GetMin(Dictionary<Variable, Variable> variables) => Value;

    public override int GetMax(Dictionary<Variable, Variable> variables) => Value;
    
    public override Variable? TryRestrictToMin(int minValue)
    {
        return minValue <= Value
            ? this
            : null;
    }
    
    public override Variable? TryRestrictToMax(int maxValue)
    {
        return Value <= maxValue
            ? this
            : null;
    }

    public override Variable? TryExclude(int value)
    {
        return Value != value
            ? this
            : null;
    }

    public override RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables)
    {
        return minValue <= Value
            ? RestrictResult.NoChange
            : RestrictResult.Infeasible;
    }
    
    public override RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables)
    {
        return Value <= maxValue
            ? RestrictResult.NoChange
            : RestrictResult.Infeasible;
    }

    public override RestrictResult Exclude(int value, Dictionary<Variable, Variable> variables)
    {
        return value == Value
            ? RestrictResult.Infeasible
            : RestrictResult.NoChange;
    }

    public override IEnumerable<Variable> GetVariables() => Enumerable.Empty<Variable>();

    public override string ToString()
    {
        return Value.ToString();
    }
}