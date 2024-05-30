namespace Solver.Lib;

public class NotEqualConstraint(Expression expression) : IConstraint
{
    public Expression Expression => expression;

    public RestrictResult Restrict(VariableCollection variables)
    {
        // e!=0  <=>  e<0 or -e<0  <=>  e<=-1 or -e<=-1  <=>  1+e<=0 or 1-e<=0

        var range = expression.GetRange(variables);
        
        if (range.Max == 0)
            return (1 + expression).RestrictToMaxZero(variables);

        if (range.Min == 0)
            return (1 - expression).RestrictToMaxZero(variables);

        return RestrictResult.NoChange;
    }

    public IEnumerable<int> GetVariableIndices()
    {
        return expression.GetVariableIndices();
    }

    public IConstraint ReduceBy(EqualityConstraint source, int variableIndex)
    {
        var sourceScale = source.GetScale(variableIndex);
        if (sourceScale == 0)
            throw new ArgumentException("The source does not contain the variable", nameof(variableIndex));

        var targetScale = expression.GetScale(variableIndex);
        if (targetScale == 0)
            return this;

        var sourceExpression = source.Expression;
        var targetExpression = expression;
        
        if (sourceScale <= targetScale)
        {
            var factor = targetScale / sourceScale;
            targetExpression -= factor * sourceExpression;
            targetScale -= factor * sourceScale;
        }

        if (targetScale != 0 && sourceScale > targetScale)
        {
            targetExpression *= Math.Abs(sourceScale);
            sourceExpression *= targetScale * Math.Sign(sourceScale);

            targetExpression = sourceExpression - targetExpression;
        }

        return new NotEqualConstraint(targetExpression);
    }

    public IConstraint EliminateConstants(VariableCollection variables)
    {
        var newExpression = expression.EliminateConstants(variables);
        if (ReferenceEquals(newExpression, expression))
            return this;
        
        return new NotEqualConstraint(newExpression);
    }

    public IEnumerable<(int variableIndex, int value)[]> GetValidOptions(VariableCollection variables)
    {
        throw new NotImplementedException();
    }

    public bool IsValid(VariableCollection variables)
    {
        var range = expression.GetRange(variables);
        if (range.TryGetConstant(out var constant))
            return constant != 0;

        return true;
    }

    public override string ToString()
    {
        return expression + " != 0";
    }
}