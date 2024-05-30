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
        problem.AddConstraint(sum == 1);

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
        problem.AddConstraint(bin.Sum() == 1);

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
        var bin = problem.AddBinaryVariables(10);
        var max = problem.AddVariable(0..10);
        problem.AddConstraint(bin.Sum() == max);

        problem.Objective = -3 * max;
        for (int i = 0; i < bin.Length; i++) 
            problem.Objective += (i - 2.5) * bin[i];
        
        var solution = problem.Minimize();
        Assume.That(!solution.IsInfeasible, "Feasible");
        Assert.That(solution.IsSolved, "Solved");
        Assert.That(solution.GetObjectiveValue(), Is.EqualTo(18.0), "objective");

        Console.WriteLine("obj: " + solution.GetObjectiveValue());
        Console.WriteLine("bin: " + String.Join(", ", solution[bin]));
        Console.WriteLine("max: " + solution[max]);
    }

    [Test]
    public void TestReduce2()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddVariables(new VariableType(-2, 4), 2);
        var (a, b) = (bin[0], bin[1]);
        problem.AddConstraint(a + b == 1);
        problem.AddConstraint(b - a == 1);

        problem.Restrict();
        Console.WriteLine("bin: " + String.Join(", ", problem[bin]));
        Assume.That(problem.IsSolved, Is.False, "reduce doesn't solve");
        
        problem.Reduce();
        problem.Restrict();
        Console.WriteLine("bin: " + String.Join(", ", problem[bin]));
        Assert.That(problem.IsSolved, Is.True, "solved");
    }

    [Test]
    public void TestReduce3()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddVariables(new VariableType(-2, 4), 3);
        var (a, b, c) = (bin[0], bin[1], bin[2]);
        problem.AddConstraint(a + b == 1);
        problem.AddConstraint(b + c == 3);
        problem.AddConstraint(a + c == 2);

        problem.Restrict();
        Console.WriteLine("bin: " + String.Join(", ", problem[bin]));
        Assume.That(problem.IsSolved, Is.False, "reduce doesn't solve");
        
        problem.Reduce();
        problem.Restrict();
        Console.WriteLine("bin: " + String.Join(", ", problem[bin]));
        Assert.That(problem.IsSolved, "solved");
    }

    [Test]
    public void TestReduce4()
    {
        var problem = new IntegerProblem();
        var bin = problem.AddBinaryVariables(4);
        var (a, b, c, d) = (bin[0], bin[1], bin[2], bin[3]);
        problem.AddConstraint(a + b + c == 2);
        problem.AddConstraint(a + b + d == 1);
        problem.AddConstraint(a + c + d == 2);
        problem.AddConstraint(b + c + d == 1);

        problem.Restrict();
        Console.WriteLine("bin: " + String.Join(", ", problem[bin]));
        Assume.That(problem.IsSolved, Is.False, "reduce doesn't solve");
        
        problem.Reduce();
        problem.Restrict();
        Console.WriteLine("bin: " + String.Join(", ", problem[bin]));
        Assert.That(problem.IsSolved, Is.True, "solved");
    }
}
