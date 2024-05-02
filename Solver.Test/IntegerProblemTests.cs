using Solver.Lib;

namespace Solver.Test;

public class IntegerProblemTests
{
    [Test]
    public void TestSimple()
    {
        var problem = new IntegerProblem();
        var bin1 = problem.AddBinaryVariable();
        var bin2 = problem.AddBinaryVariable();
        var sum = SumExpression.Create(bin1, bin2);
        problem.AddConstraint(sum, Comparison.Equals, 1);

        var x = new DistinctConstraint(bin1, bin2) { DefaultValue = 0 };

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res1 = problem[bin1];
        var res2 = problem[bin2];
        Console.WriteLine(res1 + " , " + res2);

        problem[bin1] = new ConstantVariable(1);
        result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        Console.WriteLine(problem[bin1] + " , " + problem[bin2]);
        
        Assert.That(problem.IsSolved);
    }
    
    [Test]
    public void TestOperators()
    {
        var problem = new IntegerProblem();
        var bin1 = problem.AddBinaryVariable();
        var bin2 = problem.AddBinaryVariable();
        problem.AddConstraint(bin1 + bin2 == 1);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res1 = problem[bin1];
        var res2 = problem[bin2];
        Console.WriteLine(res1 + " , " + res2);

        problem[bin1] = 1;
        result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        Console.WriteLine(problem[bin1] + " , " + problem[bin2]);
        
        Assert.That(problem.IsSolved);
    }
    
    [Test]
    public void TestInequality()
    {
        var problem = new IntegerProblem();
        var bin1 = problem.AddBinaryVariable();
        var bin2 = problem.AddBinaryVariable();
        problem.AddConstraint(bin1 != bin2);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res1 = problem[bin1];
        var res2 = problem[bin2];
        Console.WriteLine(res1 + " , " + res2);

        problem[bin1] = 1;
        result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        Console.WriteLine(problem[bin1] + " , " + problem[bin2]);
        
        Assert.That(problem.IsSolved);
    }
}
