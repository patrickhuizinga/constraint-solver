namespace Solver.Lib;

public class LessThanConstraint : IConstraint
{
    private readonly Expression _expression;

    public LessThanConstraint(Expression left, Expression right)
    {
        _expression = left - right;
    }

    public LessThanConstraint(Expression expression)
    {
        _expression = expression;
    }

    public RestrictResult Restrict(VariableCollection variables)
    {
        return _expression.RestrictToMaxZero(variables);
    }

    public IEnumerable<int> GetVariableIndices()
    {
        return _expression.GetVariableIndices();
    }
}