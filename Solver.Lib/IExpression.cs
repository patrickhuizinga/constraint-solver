namespace Solver.Lib;

public interface IExpression
{
    int GetMin(Dictionary<Variable, Variable> variables);
    int GetMax(Dictionary<Variable, Variable> variables);


    RestrictResult RestrictToMin(int minValue, Dictionary<Variable,Variable> variables);
    RestrictResult RestrictToMax(int maxValue, Dictionary<Variable,Variable> variables);
    IEnumerable<Variable> GetVariables();
}