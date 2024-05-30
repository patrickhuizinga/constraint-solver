namespace Solver.Lib;

public interface IConstraint
{
    public Expression Expression { get; }
    public int VariableCount => GetVariableIndices().Count();
    IEnumerable<int> GetVariableIndices();
    bool HasLeadingVariable(int index) => GetVariableIndices().FirstOrDefault() == index;
    
    RestrictResult Restrict(VariableCollection variables);
    IConstraint? ReduceBy(EqualityConstraint source, int variableIndex);
    IConstraint EliminateConstants(VariableCollection variables);

    int EstimateValidOptionsCount(VariableCollection variables)
    {
        var count = 1;
        foreach (var index in GetVariableIndices()) 
            count *= variables[index].Size;

        return count;
    }
    IEnumerable<(int variableIndex, int value)[]> GetValidOptions(VariableCollection variables);
    bool IsValid(VariableCollection variables);

    string ToString();
}