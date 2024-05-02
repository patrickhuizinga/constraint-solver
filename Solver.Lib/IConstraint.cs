namespace Solver.Lib;

public interface IConstraint
{
    RestrictResult Restrict(List<VariableType> variables);
    int Range(List<VariableType> variables);
    IEnumerable<int> GetVariableIndices();
}