using Solver.Lib;

namespace Solver.Test;

public class SudokuTests
{
    [Test]
    public void TwoTwoSudoku()
    {
        var problem = new IntegerProblem();
        var variables = problem.AddBinaryVariables(2, 2, 2);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);

        problem.AddConstraints(sum0, Comparison.Equals, 1);
        problem.AddConstraints(sum1, Comparison.Equals, 1);
        problem.AddConstraints(sum2, Comparison.Equals, 1);

        problem[variables[1, 0, 1]] = 1;

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        result = problem.Restrict();
        Assert.That(result, Is.EqualTo(RestrictResult.NoChange));

        for (int i = 0; i < 2; i++)
        {
            Console.Write("[");
            for (int j = 0; j < 2; j++)
            {
                if (problem[variables[i, j, 0]].Min == 1)
                    Console.Write("1, ");
                else if (problem[variables[i, j, 0]].Max == 0)
                    Console.Write("2, ");
                else
                    Console.Write("?, ");
            }

            Console.WriteLine("]");
        }
    }

    [Test]
    public void FourFourSudoku()
    {
        var problem = new IntegerProblem();
        var variables = problem.AddBinaryVariables(4, 4, 4);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);
        var sumBlocks = SumBlocks(variables, 2);

        problem.AddConstraints(sum0, Comparison.Equals, 1);
        problem.AddConstraints(sum1, Comparison.Equals, 1);
        problem.AddConstraints(sum2, Comparison.Equals, 1);
        problem.AddConstraints(sumBlocks, Comparison.Equals, 1);

        int[,] start =
        {
            { 1, 2, 0, 0, },
            { 3, 0, 0, 0, },
            { 0, 0, 4, 0, },
            { 0, 0, 0, 1, },
        };

        SetStart(problem, variables, start);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));
        Assert.That(problem.IsSolved);

        result = problem.Restrict();
        Assert.That(result, Is.EqualTo(RestrictResult.NoChange));

        for (int i = 0; i < 4; i++)
        {
            Console.Write("[");
            for (int j = 0; j < 4; j++)
            {
                if (problem[variables[i, j, 0]].Max == 1)
                    Console.Write("1");
                if (problem[variables[i, j, 1]].Max == 1)
                    Console.Write("2");
                if (problem[variables[i, j, 2]].Max == 1)
                    Console.Write("3");
                if (problem[variables[i, j, 3]].Max == 1)
                    Console.Write("4");

                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    [Test]
    public void MediumSudoku()
    {
        var problem = new IntegerProblem();
        var variables = problem.AddBinaryVariables(9, 9, 9);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);
        var sumBlocks = SumBlocks(variables, 3);

        problem.AddConstraints(sum0, Comparison.Equals, 1);
        problem.AddConstraints(sum1, Comparison.Equals, 1);
        problem.AddConstraints(sum2, Comparison.Equals, 1);
        problem.AddConstraints(sumBlocks, Comparison.Equals, 1);

        int[,] medium =
        {
            { 0, 0, 4, 0, 7, 0, 0, 9, 1 },
            { 0, 3, 0, 0, 0, 8, 0, 0, 0 },
            { 0, 6, 7, 0, 0, 0, 3, 0, 5 },
            { 0, 4, 0, 9, 3, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0, 9, 0, 0 },
            { 0, 0, 0, 8, 0, 4, 0, 3, 6 },
            { 4, 7, 0, 3, 5, 0, 6, 1, 2 },
            { 9, 2, 0, 7, 8, 1, 4, 0, 3 },
            { 5, 1, 0, 0, 0, 6, 0, 0, 0 },
        };

        SetStart(problem, variables, medium);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));
        Assert.That(problem.IsSolved);

        for (int i = 0; i < 9; i++)
        {
            Console.Write("[");
            for (int j = 0; j < 9; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (problem[variables[i, j, k]].Max == 1)
                        Console.Write(k + 1); 
                }

                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    [Test]
    public void ExpertSudoku()
    {
        var problem = new IntegerProblem();
        var variables = problem.AddBinaryVariables(9, 9, 9);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);
        var sumBlocks = SumBlocks(variables, 3);

        problem.AddConstraints(sum0, Comparison.Equals, 1);
        problem.AddConstraints(sum1, Comparison.Equals, 1);
        problem.AddConstraints(sum2, Comparison.Equals, 1);
        problem.AddConstraints(sumBlocks, Comparison.Equals, 1);

        int[,] extreme =
        {
            { 4, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 3, 0, 0, 0, 0, 2, 9 },
            { 0, 0, 0, 0, 0, 4, 0, 6, 0 },
            { 0, 0, 0, 7, 0, 0, 0, 9, 0 },
            { 0, 0, 7, 4, 0, 0, 0, 0, 2 },
            { 0, 0, 0, 0, 0, 8, 0, 0, 5 },
            { 6, 7, 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 9, 0, 2, 0, 0, 0, 3 },
            { 0, 3, 0, 0, 0, 9, 0, 5, 0 },
        };

        SetStart(problem, variables, extreme);

        var isSolved = problem.FindFeasible();
        Assert.That(isSolved);
        Assert.That(problem.IsSolved);

        for (int i = 0; i < 9; i++)
        {
            Console.Write("[");
            for (int j = 0; j < 9; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (problem[variables[i, j, k]].Max == 1)
                        Console.Write(k + 1); 
                }

                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    private static void SetStart(IntegerProblem integerProblem, BinaryVariable[,,] variables, int[,] start)
    {
        var countI = start.GetLength(0);
        var countJ = start.GetLength(1);
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            if (start[i, j] == 0)
                continue;
            
            var k = start[i, j] - 1;
            integerProblem[variables[i, j, k]] = 1;
        }
    }

    [Test]
    public void OneColdSudoku()
    {
        // Natural way to encode a sudoku is with a one-hot encoding; a binary variable that is 1 when that digit is set
        // With 'one-cold' encoding; all digits except the answer is set to 1.
        
        var problem = new IntegerProblem();
        var variables = problem.AddBinaryVariables(9, 9, 9);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);
        var sumBlocks = SumBlocks(variables, 3);

        problem.AddConstraints(sum0, Comparison.Equals, 8);
        problem.AddConstraints(sum1, Comparison.Equals, 8);
        problem.AddConstraints(sum2, Comparison.Equals, 8);
        problem.AddConstraints(sumBlocks, Comparison.Equals, 8);

        int[,] medium =
        {
            { 0, 0, 4, 0, 7, 0, 0, 9, 1 },
            { 0, 3, 0, 0, 0, 8, 0, 0, 0 },
            { 0, 6, 7, 0, 0, 0, 3, 0, 5 },
            { 0, 4, 0, 9, 3, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0, 9, 0, 0 },
            { 0, 0, 0, 8, 0, 4, 0, 3, 6 },
            { 4, 7, 0, 3, 5, 0, 6, 1, 2 },
            { 9, 2, 0, 7, 8, 1, 4, 0, 3 },
            { 5, 1, 0, 0, 0, 6, 0, 0, 0 },
        };

        SetOneColdStart(problem, variables, medium);

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));
        Assert.That(problem.IsSolved);

        for (int i = 0; i < 9; i++)
        {
            Console.Write("[");
            for (int j = 0; j < 9; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (problem[variables[i, j, k]].Min == 0)
                        Console.Write(k + 1); 
                }

                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    private static void SetOneColdStart(IntegerProblem integerProblem, BinaryVariable[,,] variables, int[,] start)
    {
        var countI = start.GetLength(0);
        var countJ = start.GetLength(1);
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            if (start[i, j] == 0)
                continue;
            
            var k = start[i, j] - 1;
            integerProblem[variables[i, j, k]] = 0;
        }
    }

    private static Expression[,] Sum<T>(T[,,] expressions, int dimension) where T : Expression
    {
        var countI = expressions.GetLength(0);
        var countJ = expressions.GetLength(1);
        var countK = expressions.GetLength(2);

        Expression[,] result;

        switch (dimension)
        {
            case 0:
                result = new Expression[countJ, countK];
                for (int j = 0; j < countJ; j++)
                for (int k = 0; k < countK; k++)
                {
                    var parts = new Expression[countI];
                    for (int i = 0; i < countI; i++)
                        parts[i] = expressions[i, j, k];

                    result[j, k] = new SumExpression(parts);
                }

                return result;
            case 1:
                result = new Expression[countI, countK];
                for (int i = 0; i < countI; i++)
                for (int k = 0; k < countK; k++)
                {
                    var parts = new Expression[countJ];
                    for (int j = 0; j < countJ; j++)
                        parts[j] = expressions[i, j, k];

                    result[i, k] = new SumExpression(parts);
                }

                return result;
            case 2:
                result = new Expression[countI, countJ];
                for (int i = 0; i < countI; i++)
                for (int j = 0; j < countJ; j++)
                {
                    var parts = new Expression[countK];
                    for (int k = 0; k < countK; k++)
                        parts[k] = expressions[i, j, k];

                    result[i, j] = new SumExpression(parts);
                }

                return result;
            default:
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Dimension should be 0, 1, or 2");
        }
    }

    private static Expression[,] SumBlocks<T>(T[,,] expressions, int size) where T : Expression
    {
        var countK = size * size;

        var result = new Expression[size * size, countK];

        for (int bi = 0; bi < size; bi++)
        for (int bj = 0; bj < size; bj++)
        for (int k = 0; k < countK; k++)
        {
            var parts = new Expression[countK];
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                parts[i * size + j] = expressions[bi * size + i, bj * size + j, k];

            result[bi * size + bj, k] = new SumExpression(parts);
        }

        return result;
    }
}
