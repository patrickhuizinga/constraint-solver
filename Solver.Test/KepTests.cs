using Solver.Lib;

namespace Solver.Test;

public class KepTests
{
    [Test]
    public void TinyLowDensity()
    {
        int k = 3;
        int n = 10;
        var A = new bool[n, n];
        var w = new double[n, n];
        var rng = new Random(42);
        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            if (i == j) continue;
            if (rng.Next(5) > 0) continue;

            A[i, j] = true;
            w[i, j] = rng.NextDouble();
        }

        var solution = Minimize(A, w, k);
        
        Console.WriteLine("objective: " + -solution.GetObjectiveValue());
    }

    private static IntegerProblem Minimize(bool[,] A, double[,] w, int k)
    {
        var n = A.LengthI();
        int L = n;

        var problem = new IntegerProblem();
        var x = problem.AddBinaryVariables(A);
        var y = problem.AddBinaryVariables(n, L);

        problem.Objective = -SumSum(w, x);

        problem.AddConstraints(Sum(x, 0), Comparison.Equals, Sum(x, 1));
        problem.AddConstraints(Sum(x, 1), Comparison.LessEqual, 1);
        problem.AddConstraints(Sum(y, 0), Comparison.LessEqual, k);
        problem.AddConstraints(Sum(y, 1), Comparison.Equals, Sum(x, 1));
        foreach (var (i, j) in Indices(A))
            for (int l = 0; l < L; l++) 
                problem.AddConstraint(y[i, l] + x[i, j] <= 1 + y[j, l]);

        var solution = problem.Minimize();
        return solution;
    }

    private static IEnumerable<(int i, int j)> Indices(bool[,] enabled)
    {
        var (lengthI, lengthJ) = enabled.Dim();
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (enabled[i, j]) yield return (i, j);
        }
    }

    private static Expression[] Sum(Variable[,] variables, int dimension)
    {
        var (lengthI, lengthJ) = variables.Dim();

        Expression[] result;

        switch (dimension)
        {
            case 0:
                result = new Expression[lengthJ];
                for (int j = 0; j < lengthJ; j++)
                {
                    var parts = new Variable[lengthI];
                    for (int i = 0; i < lengthI; i++)
                        parts[i] = variables[i, j];

                    result[j] = SumExpression.Create(parts);
                }

                return result;
            case 1:
                result = new Expression[lengthI];
                for (int i = 0; i < lengthI; i++)
                {
                    var parts = new Variable[lengthJ];
                    for (int j = 0; j < lengthJ; j++)
                        parts[j] = variables[i, j];

                    result[i] = SumExpression.Create(parts);
                }

                return result;
            default:
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Dimension should be 0, 1, or 2");
        }
    }

    private static SumDoubleExpression SumSum(double[,] weights, Variable[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var scales = new double[variables.Length];
        var parts = new Variable[variables.Length];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            scales[i * lengthJ + j] = weights[i, j];
            parts[i * lengthJ + j] = variables[i, j];
        }

        return new SumDoubleExpression(parts, scales);
    }
}