namespace Solver.Lib;

public interface IConstraint
{
    RestrictResult Restrict(IList<VariableType> variables);
    IEnumerable<int> GetVariableIndices();
}