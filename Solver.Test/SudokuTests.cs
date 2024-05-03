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

        problem[variables[1, 0, 1]] = VariableType.True;

        var result = problem.Restrict();
        Assert.That(result, Is.Not.EqualTo(RestrictResult.Infeasible));

        result = problem.Restrict();
        Assert.That(result, Is.EqualTo(RestrictResult.NoChange));

        PrintSudoku(problem[variables]);
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

        PrintSudoku(problem[variables]);
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

        PrintSudoku(problem[variables]);
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

        problem.Restrict();
        PrintSudoku(problem[variables]);
        Console.WriteLine();

        var isSolved = problem.FindFeasible();
        Assert.That(isSolved);
        Assert.That(problem.IsSolved);

        PrintSudoku(problem[variables]);
    }

    private static void SetStart(IntegerProblem problem, Variable[,,] variables, int[,] start)
    {
        var countI = start.GetLength(0);
        var countJ = start.GetLength(1);
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            if (start[i, j] == 0)
                continue;
            
            var k = start[i, j] - 1;
            problem[variables[i, j, k]] = VariableType.True;
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

        var values = problem[variables];
        for (int i = 0; i < 9; i++)
        {
            Console.Write("[");
            for (int j = 0; j < 9; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    if (values[i, j, k].Min == 0)
                        Console.Write(k + 1); 
                }

                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    private static void SetOneColdStart(IntegerProblem integerProblem, Variable[,,] variables, int[,] start)
    {
        var countI = start.GetLength(0);
        var countJ = start.GetLength(1);
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            if (start[i, j] == 0)
                continue;
            
            var k = start[i, j] - 1;
            integerProblem[variables[i, j, k]] = VariableType.False;
        }
    }

    [Test, Ignore("constraints not implemented strongly enough")]
    public void RangeSudoku()
    {   
        var problem = new IntegerProblem();
        var variables = problem.AddVariables(1..9, 9, 9);
        var distinctRows = Distinct(variables, 0);
        var distinctCols = Distinct(variables, 1);
        var distinctBlocks = DistinctBlocks(variables, 3);

        problem.AddConstraints(distinctRows);
        problem.AddConstraints(distinctCols);
        problem.AddConstraints(distinctBlocks);

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
                Console.Write(problem[variables[i, j]]);
                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    private static void SetStart(IntegerProblem problem, Variable[,] variables, int[,] start)
    {
        var countI = start.GetLength(0);
        var countJ = start.GetLength(1);
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            if (start[i, j] == 0)
                continue;
            
            problem[variables[i, j]] = start[i, j];
        }
    }

    [Test]
    public void EqualitySudoku()
    {
        var problem = new EqualityProblem();
        var variables = problem.AddBinaryVariables(9, 9, 9);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);
        var sumBlocks = SumBlocks(variables, 3);

        problem.AddConstraints(sum0, 1);
        problem.AddConstraints(sum1, 1);
        problem.AddConstraints(sum2, 1);
        problem.AddConstraints(sumBlocks, 1);

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

        PrintSudoku(problem[variables]);
    }

    [Test]
    public void ExpertEqualitySudoku()
    {
        var problem = new EqualityProblem();
        var variables = problem.AddBinaryVariables(9, 9, 9);
        var sum0 = Sum(variables, 0);
        var sum1 = Sum(variables, 1);
        var sum2 = Sum(variables, 2);
        var sumBlocks = SumBlocks(variables, 3);

        problem.AddConstraints(sum0, 1);
        problem.AddConstraints(sum1, 1);
        problem.AddConstraints(sum2, 1);
        problem.AddConstraints(sumBlocks, 1);

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

        problem.Restrict();
        PrintSudoku(problem[variables]);
        Console.WriteLine();

        var isSolved = problem.FindFeasible();
        Assert.That(isSolved);
        Assert.That(problem.IsSolved);

        PrintSudoku(problem[variables]);
    }

    private static void SetStart(EqualityProblem problem, Variable[,,] variables, int[,] start)
    {
        var countI = start.GetLength(0);
        var countJ = start.GetLength(1);
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            if (start[i, j] == 0)
                continue;
            
            var k = start[i, j] - 1;
            problem[variables[i, j, k]] = VariableType.True;
        }
    }

    private static void PrintSudoku(VariableType[,,] values)
    {
        var lengthI = values.GetLength(0);
        var lengthJ = values.GetLength(1);
        var lengthK = values.GetLength(2);
        
        for (int i = 0; i < lengthI; i++)
        {
            Console.Write("[");
            for (int j = 0; j < lengthJ; j++)
            {
                for (int k = 0; k < lengthK; k++)
                {
                    if (values[i, j, k].Max == 1)
                        Console.Write(k + 1); 
                }

                Console.Write(", ");
            }

            Console.WriteLine("]");
        }
    }

    private static Expression[,] Sum<TExpression>(TExpression[,,] expressions, int dimension) where TExpression : Expression
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

                    result[j, k] = SumExpression.Create(parts);
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

                    result[i, k] = SumExpression.Create(parts);
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

                    result[i, j] = SumExpression.Create(parts);
                }

                return result;
            default:
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Dimension should be 0, 1, or 2");
        }
    }

    private static Expression[,] SumBlocks<TExpression>(TExpression[,,] expressions, int size) where TExpression : Expression
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

            result[bi * size + bj, k] = SumExpression.Create(parts);
        }

        return result;
    }

    private static DistinctConstraint[] Distinct(Variable[,] expressions, int dimension)
    {
        var countI = expressions.GetLength(0);
        var countJ = expressions.GetLength(1);

        DistinctConstraint[] result;

        switch (dimension)
        {
            case 0:
                result = new DistinctConstraint[countJ];
                for (int j = 0; j < countJ; j++)
                {
                    var parts = new Variable[countI];
                    for (int i = 0; i < countI; i++)
                        parts[i] = expressions[i, j];

                    result[j] = new DistinctConstraint(parts) { DefaultValue = 0 };
                }

                return result;
            case 1:
                result = new DistinctConstraint[countI];
                for (int i = 0; i < countI; i++)
                {
                    var parts = new Variable[countJ];
                    for (int j = 0; j < countJ; j++)
                        parts[j] = expressions[i, j];

                    result[i] = new DistinctConstraint(parts) { DefaultValue = 0 };
                }

                return result;
            default:
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Dimension should be 0, 1, or 2");
        }
    }

    private static DistinctConstraint[,] Distinct(Variable[,,] expressions, int dimension)
    {
        var countI = expressions.GetLength(0);
        var countJ = expressions.GetLength(1);
        var countK = expressions.GetLength(2);

        DistinctConstraint[,] result;

        switch (dimension)
        {
            case 0:
                result = new DistinctConstraint[countJ, countK];
                for (int j = 0; j < countJ; j++)
                for (int k = 0; k < countK; k++)
                {
                    var parts = new Variable[countI];
                    for (int i = 0; i < countI; i++)
                        parts[i] = expressions[i, j, k];

                    result[j, k] = new DistinctConstraint(parts) { DefaultValue = 0 };
                }

                return result;
            case 1:
                result = new DistinctConstraint[countI, countK];
                for (int i = 0; i < countI; i++)
                for (int k = 0; k < countK; k++)
                {
                    var parts = new Variable[countJ];
                    for (int j = 0; j < countJ; j++)
                        parts[j] = expressions[i, j, k];

                    result[i, k] = new DistinctConstraint(parts) { DefaultValue = 0 };
                }

                return result;
            case 2:
                result = new DistinctConstraint[countI, countJ];
                for (int i = 0; i < countI; i++)
                for (int j = 0; j < countJ; j++)
                {
                    var parts = new Variable[countK];
                    for (int k = 0; k < countK; k++)
                        parts[k] = expressions[i, j, k];

                    result[i, j] = new DistinctConstraint(parts) { DefaultValue = 0 };
                }

                return result;
            default:
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Dimension should be 0, 1, or 2");
        }
    }

    private static DistinctConstraint[] DistinctBlocks(Variable[,] expressions, int size)
    {
        var countK = size * size;

        var result = new DistinctConstraint[size * size];

        for (int bi = 0; bi < size; bi++)
        for (int bj = 0; bj < size; bj++)
        {
            var parts = new Variable[countK];
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                parts[i * size + j] = expressions[bi * size + i, bj * size + j];

            result[bi * size + bj] = new DistinctConstraint(parts) { DefaultValue = 0 };
        }

        return result;
    }

    private static DistinctConstraint[,] DistinctBlocks(Variable[,,] expressions, int size)
    {
        var countK = size * size;

        var result = new DistinctConstraint[size * size, countK];

        for (int bi = 0; bi < size; bi++)
        for (int bj = 0; bj < size; bj++)
        for (int k = 0; k < countK; k++)
        {
            var parts = new Variable[countK];
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                parts[i * size + j] = expressions[bi * size + i, bj * size + j, k];

            result[bi * size + bj, k] = new DistinctConstraint(parts) { DefaultValue = 0 };
        }

        return result;
    }
}
