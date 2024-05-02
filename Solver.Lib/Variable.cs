namespace Solver.Lib;

public abstract class Variable : Expression
{
    public abstract int Min { get; }
    public abstract int Max { get; }

    public override RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables)
    {
        var oldVal = variables[this];
        var newVal = oldVal.TryRestrictToMin(minValue);
        if (ReferenceEquals(newVal, null))
            return RestrictResult.Infeasible;
        if (ReferenceEquals(newVal, oldVal))
            return RestrictResult.NoChange;

        variables[this] = newVal;
        return RestrictResult.Change;
    }

    public override RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables)
    {
        var oldVal = variables[this];
        var newVal = oldVal.TryRestrictToMax(maxValue);
        if (ReferenceEquals(newVal, null))
            return RestrictResult.Infeasible;
        if (ReferenceEquals(newVal, oldVal))
            return RestrictResult.NoChange;

        variables[this] = newVal;
        return RestrictResult.Change;
    }

    public override RestrictResult Exclude(int value, Dictionary<Variable, Variable> variables)
    {
        var oldVal = variables[this];
        var newVal = oldVal.TryExclude(value);
        if (ReferenceEquals(newVal, null))
            return RestrictResult.Infeasible;
        if (ReferenceEquals(newVal, oldVal))
            return RestrictResult.NoChange;

        variables[this] = newVal;
        return RestrictResult.Change;
    }

    public abstract Variable? TryRestrictToMin(int minValue);
    public abstract Variable? TryRestrictToMax(int maxValue);
    public abstract Variable? TryExclude(int value);

    public virtual int ValueCount => Max - Min;

    public virtual IEnumerable<int> GetValues()
    {
        var min = Min;
        return Enumerable.Range(min, Max - min);
    }

    public virtual bool Contains(int value)
    {
        return Min <= value && value <= Max;
    }
    
    public override IEnumerable<Variable> GetVariables() => new[] { this };

    public static implicit operator Variable(int value)
    {
        return new ConstantVariable(value);
    }
}