namespace Solver.Lib;

public class Variable(int index) : Expression
{
    public int Index { get; } = index;

    public override int GetMin(IList<VariableType> variables) => GetMin(Index, variables);

    public override int GetMax(IList<VariableType> variables) => GetMax(Index, variables);

    public override RestrictResult RestrictToMin(int minValue, IList<VariableType> variables)
    {
        return RestrictToMin(Index, minValue, variables);
    }

    public override RestrictResult RestrictToMax(int maxValue, IList<VariableType> variables)
    {
        return RestrictToMax(Index, maxValue, variables);
    }

    public static int GetMin(int index, IList<VariableType> variables) => variables[index].Min;

    public static int GetMax(int index, IList<VariableType> variables) => variables[index].Max;

    public static RestrictResult RestrictToMin(int index, int minValue, IList<VariableType> variables)
    {
        var oldVal = variables[index];
        var newVal = oldVal.TryRestrictToMin(minValue);
        if (newVal == null)
            return RestrictResult.Infeasible;
        if (newVal == oldVal)
            return RestrictResult.NoChange;

        variables[index] = newVal;
        return RestrictResult.Change;
    }

    public static RestrictResult RestrictToMax(int index, int maxValue, IList<VariableType> variables)
    {
        var oldVal = variables[index];
        var newVal = oldVal.TryRestrictToMax(maxValue);
        if (ReferenceEquals(newVal, null))
            return RestrictResult.Infeasible;
        if (ReferenceEquals(newVal, oldVal))
            return RestrictResult.NoChange;

        variables[index] = newVal;
        return RestrictResult.Change;
    }

    public static RestrictResult Exclude(int index, int value, IList<VariableType> variables)
    {
        var oldVal = variables[index];
        var newVal = oldVal.TryExclude(value);
        if (ReferenceEquals(newVal, null))
            return RestrictResult.Infeasible;
        if (ReferenceEquals(newVal, oldVal))
            return RestrictResult.NoChange;

        variables[index] = newVal;
        return RestrictResult.Change;
    }

    public override IEnumerable<int> GetVariableIndices() => new[] { Index };
}