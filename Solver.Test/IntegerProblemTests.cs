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
        problem.AddConstraint(new EqualityConstraint(sum, 1));

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        var res = problem[bin];
        Console.WriteLine(res[0] + " , " + res[1]);

        problem[bin[0]] = VariableType.Constant(1);
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

        problem[bin[0]] = VariableType.True;
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

        problem[bin1] = VariableType.True;
        result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        Console.WriteLine(problem[bin1] + " , " + problem[bin2]);
        
        Assert.That(problem.IsSolved);
    }

    [Test]
    public void TestSimpleMinimization()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddBinaryVariables(2);
        var sum = SumExpression.Create(bin);
        problem.AddConstraint(sum == 1);

        problem.Objective = 2.0 * bin[0] + 3.0 * bin[1];
        var solution = problem.Minimize();
        Assert.That(solution.GetObjectiveValue(), Is.EqualTo(2));

        var res = solution[bin];
        Console.WriteLine(res[0] + " , " + res[1]);
    }

    [Test]
    public void TestSimpleMinimization2()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddBinaryVariables(20);
        var max = problem.AddVariable(0..10);
        var sum = SumExpression.Create(bin);
        problem.AddConstraint(sum == max);

        problem.Objective = -Double.Pi * max;
        for (int i = 0; i < bin.Length; i++) 
            problem.Objective += (i - 2.5) * bin[i];
        
        var solution = problem.Minimize();
        // Assert.That(solution.IsSolved, "Solved");
        Assert.That(!solution.IsInfeasible, "Feasible");

        Console.WriteLine("obj: " + solution.GetObjectiveValue());
        Console.WriteLine("bin: " + String.Join(", ", solution[bin]));
        Console.WriteLine("max: " + solution[max]);
    }
}
