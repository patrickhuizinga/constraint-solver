namespace Solver.Lib;

public class NotEqualConstraint : IConstraint
{
    private readonly Expression _expression;

    public NotEqualConstraint(Expression left, Expression right)
    {
        _expression = left - right;
    }

    public NotEqualConstraint(Expression expression)
    {
        _expression = expression;
    }

    public RestrictResult Restrict(VariableCollection variables)
    {
        // e!=0  <=>  e<0 or -e<0  <=>  e<=-1 or -e<=-1  <=>  1+e<=0 or 1-e<=0
        
        if (_expression.GetMax(variables) == 0)
            return (1 + _expression).RestrictToMaxZero(variables);

        if (_expression.GetMin(variables) == 0)
            return (1 - _expression).RestrictToMaxZero(variables);

        return RestrictResult.NoChange;
    }

    public IEnumerable<int> GetVariableIndices()
    {
        return _expression.GetVariableIndices();
    }
}