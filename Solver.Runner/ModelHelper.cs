using Gurobi;
using Solver.Lib;

namespace Solver.Runner;

public static class ModelHelper
{
    public static (bool[,] A, double[,] w) CreateCompatibility(int n, double density, bool realWeights, int seed)
    {
        var A = new bool[n, n];
        var w = new double[n, n];
        var rng = new Random(seed);
        
        // create the array by start top left and then expand to the bottom right by adding 'rings' / L-shapes
        // that way you ensure a larger size always improves the objective
        for (int i = 0; i < n; i++)
        for (int j = 0; j < i; j++)
        {
            if (rng.NextDouble() < density)
            {
                A[i, j] = true;
                w[i, j] = rng.NextDouble();
            }

            if (rng.NextDouble() < density)
            {
                A[j, i] = true;
                w[j, i] = rng.NextDouble();
            }
        }

        // we generate the weights even if we don't need them to force the RNG to generate the values
        // and thereby keeping the A matrix the same
        if (!realWeights)
            w = ToDouble(A);

        return (A, w);
    }

    public static double[,] ToBinary(double[,] weights)
    {
        var (lengthI, lengthJ) = weights.Dim();
        var result = new double[lengthI, lengthJ];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            result[i, j] = weights[i, j] == 0 ? 0 : 1;

        return result;
    }

    public static double[,] ToDouble(bool[,] b)
    {
        var (lengthI, lengthJ) = b.Dim();
        var result = new double[lengthI, lengthJ];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            result[i, j] = b[i, j] ? 1 : 0;

        return result;
    }

    public static IEnumerable<(int i, int j)> Indices(bool[,] enabled)
    {
        var (lengthI, lengthJ) = enabled.Dim();

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (enabled[i, j]) yield return (i, j);
        }
    }

    public static GRBLinExpr SumSum(double[,] weights, GRBVar?[,] variables)
    {
        // return variables.SumSum();
        var (lengthI, lengthJ) = variables.Dim();

        var sum = new GRBLinExpr();
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (!ReferenceEquals(variables[i, j], null))
                sum.AddTerm(weights[i, j], variables[i, j]);
        }

        return sum;
    }

    public static GRBLinExpr SumSum(double[,] weights, GRBLinExpr?[,] expressions)
    {
        // return expressions.SumSum();
        var (lengthI, lengthJ) = expressions.Dim();

        var sum = new GRBLinExpr();
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (!ReferenceEquals(expressions[i, j], null))
                sum.MultAdd(weights[i, j], expressions[i, j]);
        }

        return sum;
    }

    public static bool[,] TrueUpper(int n)
    {
        var result = new bool[n, n];

        for (int i = 0; i < n; i++)
        for (int j = i; j < n; j++)
            result[i, j] = true;
        
        return result;
    }
    

    public static bool[,,] Duplicate(bool[,] array, int lengthK)
    {
        var (lengthI, lengthJ) = array.Dim();

        var result = new bool[lengthI, lengthJ, lengthK];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
            result[i, j, k] = array[i, j];

        return result;
    }
}