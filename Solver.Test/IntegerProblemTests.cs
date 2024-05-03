using Solver.Lib;

namespace Solver.Test;

public class IntegerProblemTests
{
    [Test]
    public void TestSimple()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddBinaryVariables(2);
        var sum = SumExpression.Create(bin);
        problem.AddConstraint(sum, Comparison.Equals, 1);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res = problem[bin];
        Console.WriteLine(res[0] + " , " + res[1]);

        problem[bin[0]] = new ConstantType(1);
        result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        Console.WriteLine(problem[bin[0]] + " , " + problem[bin[1]]);
        
        Assert.That(problem.IsSolved);
    }
    
    [Test]
    public void TestOperators()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddBinaryVariables(2);
        problem.AddConstraint(bin[0] + bin[1] == 1);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res = problem[bin];
        Console.WriteLine(res[0] + " , " + res[1]);

        problem[bin[0]] = 1;
        result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        Console.WriteLine(problem[bin[0]] + " , " + problem[bin[1]]);
        
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
