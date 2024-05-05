namespace Solver.Lib;

public interface IConstraint
{
    RestrictResult Restrict(VariableCollection variables);
    IEnumerable<int> GetVariableIndices();
}