namespace Solver.Lib;

public class Constraint
{
    public Expression Left { get; }
    public Comparison Comparison { get; }
    public Expression Right { get; }

    public Constraint(Expression left, Comparison comparison, Expression right)
    {
        Left = left;
        Comparison = comparison;
        Right = right;
    }

    public RestrictResult Restrict(Dictionary<Variable,Variable> variables)
    {
        switch (Comparison)
        {
            case Comparison.LessThan:
                return RestrictLessThan(variables);
            case Comparison.GreaterThan:
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

    private RestrictResult RestrictLessThan(Dictionary<Variable, Variable> variables)
    {
        var rightMax = Right.GetMax(variables);
        var leftResult = Left.RestrictToMax(rightMax, variables);
        var leftMin = Left.GetMin(variables);
        var rightResult = Right.RestrictToMin(leftMin, variables);

        return Combine(leftResult, rightResult);
    }

    private RestrictResult RestrictGreaterThan(Dictionary<Variable, Variable> variables)
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

    public int Range(Dictionary<Variable, Variable> variables)
    {
        switch (Comparison)
        {
            case Comparison.LessThan:
                return Left.GetMax(variables) - Right.GetMin(variables);
            case Comparison.GreaterThan:
                return Right.GetMax(variables) - Left.GetMin(variables);
            case Comparison.Equals:
                return (Left.GetMax(variables) - Left.GetMin(variables))
                       + (Right.GetMax(variables) - Right.GetMin(variables));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IEnumerable<Variable> GetVariables()
    {
        foreach (var variable in Left.GetVariables())
            yield return variable;
        foreach (var variable in Right.GetVariables())
            yield return variable;
    }
}