namespace Solver.Lib;

public interface IConstraint
{
    RestrictResult Restrict(Dictionary<Variable, Variable> variables);
    int Range(Dictionary<Variable, Variable> variables);
    IEnumerable<Variable> GetVariables();
}