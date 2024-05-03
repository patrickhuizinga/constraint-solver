namespace Solver.Lib;

public class ComparisonConstraint : IConstraint
{
    public Expression Left { get; }
    public Comparison Comparison { get; }
    public Expression Right { get; }

    private ComparisonConstraint(Expression left, Comparison comparison, Expression right)
    {
        Left = left;
        Comparison = comparison;
        Right = right;
    }

    public static IConstraint Create(Expression left, Comparison comparison, Expression right)
    {
        if (comparison == Comparison.Equals)
        {
            return new EqualityConstraint(left, right);
        }

        return new ComparisonConstraint(left, comparison, right);
    }

    public RestrictResult Restrict(IList<VariableType> variables)
    {
        switch (Comparison)
        {
            case Comparison.LessEqual:
                return RestrictLessThan(variables);
            case Comparison.GreaterEqual:
                return RestrictGreaterThan(variables);
            case Comparison.Equals:
                return Combine(
                    RestrictLessThan(variables),
                    RestrictGreaterThan(variables));
            case Comparison.NotEquals:
                var rightMin = Right.GetMin(variables);
                var rightMax = Right.GetMax(variables);
                if (rightMin == rightMax)
                {
                    return Left.Exclude(rightMin, variables);
                }
                
                var leftMin = Left.GetMin(variables);
                var leftMax = Left.GetMax(variables);
                if (leftMin == leftMax)
                {
                    return Right.Exclude(leftMin, variables);
                }

                return RestrictResult.NoChange;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private RestrictResult RestrictLessThan(IList<VariableType> variables)
    {
        var rightMax = Right.GetMax(variables);
        var leftResult = Left.RestrictToMax(rightMax, variables);
        var leftMin = Left.GetMin(variables);
        var rightResult = Right.RestrictToMin(leftMin, variables);

        return Combine(leftResult, rightResult);
    }

    private RestrictResult RestrictGreaterThan(IList<VariableType> variables)
    {
        var rightMin = Right.GetMin(variables);
        var leftResult = Left.RestrictToMin(rightMin, variables);
        var leftMax = Left.GetMax(variables);
        var rightResult = Right.RestrictToMax(leftMax, variables);

        return Combine(leftResult, rightResult);
    }

    private static RestrictResult Combine(RestrictResult leftResult, RestrictResult rightResult)
    {
        if (leftResult == RestrictResult.Infeasible || rightResult == RestrictResult.Infeasible)
            return RestrictResult.Infeasible;

        if (leftResult == RestrictResult.Change || rightResult == RestrictResult.Change)
            return RestrictResult.Change;

        return RestrictResult.NoChange;
    }

    public int Range(IList<VariableType> variables)
    {
        switch (Comparison)
        {
            case Comparison.LessEqual:
                return Left.GetMax(variables) - Right.GetMin(variables);
            case Comparison.GreaterEqual:
                return Right.GetMax(variables) - Left.GetMin(variables);
            case Comparison.Equals:
                return (Left.GetMax(variables) - Left.GetMin(variables))
                       + (Right.GetMax(variables) - Right.GetMin(variables));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IEnumerable<int> GetVariableIndices()
    {
        foreach (var variable in Left.GetVariableIndices())
            yield return variable;
        foreach (var variable in Right.GetVariableIndices())
            yield return variable;
    }
}