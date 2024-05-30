using Gurobi;
using Solver.Lib;

namespace Solver.Gurobi;

public static class LinExprExtensions
{
    public static GRBLinExpr Sum(this IEnumerable<GRBLinExpr?> expressions)
    {
        var sum = new GRBLinExpr();
        foreach (var expr in expressions)
        {
            if (!ReferenceEquals(expr, null))
                sum.Add(expr);
        }

        return sum;
    }
    
    public static GRBLinExpr[] SumOverI(this GRBLinExpr?[,] expressions)
    {
        var (lengthI, lengthJ) = expressions.Dim();

        var result = new GRBLinExpr[lengthJ];
        for (int j = 0; j < lengthJ; j++)
        {
            var sum = new GRBLinExpr();
            for (int i = 0; i < lengthI; i++)
            {
                if (!ReferenceEquals(expressions[i, j], null))
                    sum.Add(expressions[i, j]);
            }

            result[j] = sum;
        }

        return result;
    }

    public static GRBLinExpr[] SumOverJ(this GRBLinExpr?[,] expressions)
    {
        var (lengthI, lengthJ) = expressions.Dim();

        var result = new GRBLinExpr[lengthI];
        for (int i = 0; i < lengthI; i++)
        {
            var sum = new GRBLinExpr();
            for (int j = 0; j < lengthJ; j++)
            {
                if (!ReferenceEquals(expressions[i, j], null))
                    sum.Add(expressions[i, j]);
            }

            result[i] = sum;
        }

        return result;
    }

    public static GRBLinExpr SumSum(this GRBLinExpr?[,] expressions)
    {
        var (lengthI, lengthJ) = expressions.Dim();

        var sum = new GRBLinExpr();
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (!ReferenceEquals(expressions[i, j], null))
                sum.Add(expressions[i, j]);
        }

        return sum;
    }
}