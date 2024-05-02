namespace Solver.Lib;

public class Variable(int index) : Expression
{
    public int Index { get; } = index;

    public override int GetMin(List<VariableType> variables) => GetMin(Index, variables);

    public override int GetMax(List<VariableType> variables) => GetMax(Index, variables);

    public override RestrictResult RestrictToMin(int minValue, List<VariableType> variables)
    {
        return RestrictToMin(Index, minValue, variables);
    }

    public override RestrictResult RestrictToMax(int maxValue, List<VariableType> variables)
    {
        return RestrictToMax(Index, maxValue, variables);
    }

    public override RestrictResult Exclude(int value, List<VariableType> variables)
    {
        return Exclude(Index, value, variables);
    }

    public static int GetMin(int index, List<VariableType> variables) => variables[index].Min;

    public static int GetMax(int index, List<VariableType> variables) => variables[index].Max;

    public static RestrictResult RestrictToMin(int index, int minValue, List<VariableType> variables)
    {
        var oldVal = variables[index];
        var newVal = oldVal.TryRestrictToMin(minValue);
        if (ReferenceEquals(newVal, null))
            return RestrictResult.Infeasible;
        if (ReferenceEquals(newVal, oldVal))
            return RestrictResult.NoChange;

        variables[index] = newVal;
        return RestrictResult.Change;
    }

    public static RestrictResult RestrictToMax(int index, int maxValue, List<VariableType> variables)
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

    public static RestrictResult Exclude(int index, int value, List<VariableType> variables)
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