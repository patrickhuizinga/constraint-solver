using Solver.Lib;

namespace Solver.Test;

public class IntegerProblemTests
{
    [Test]
    public void Test1()
    {
        var problem = new IntegerProblem();
        var bin1 = problem.AddBinaryVariable();
        var bin2 = problem.AddBinaryVariable();
        var sum = new SumExpression(bin1, bin2);
        problem.AddConstraint(sum, Comparison.Equals, 1);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res1 = problem[bin1];
        var res2 = problem[bin2];
        Console.WriteLine(res1 + " , " + res2);
    }
}
