namespace Solver.Lib;

public static class Constraint
{
    public static IConstraint Create(Expression left, Comparison comparison, Expression right)
    {
        return comparison switch
        {
            Comparison.Equals => new EqualityConstraint(left, right),
            Comparison.LessEqual => new LessThanConstraint(left, right),
            Comparison.GreaterEqual => new LessThanConstraint(right, left),
            Comparison.NotEquals => new NotEqualConstraint(left, right),
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, "Unsupported comparison type")
        };
    }
}