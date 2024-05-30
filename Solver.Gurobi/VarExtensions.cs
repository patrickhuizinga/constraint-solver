using Gurobi;
using Solver.Lib;

namespace Solver.Gurobi;

public static class VarExtensions
{
    public static GRBLinExpr Sum(this GRBVar?[] variables)
    {
        var sum = new GRBLinExpr();
        foreach (var v in variables)
        {
            if (!ReferenceEquals(v, null))
                sum.AddTerm(1, v);
        }

        return sum;
    }
    
    public static GRBLinExpr[] SumOverI(this GRBVar?[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var result = new GRBLinExpr[lengthJ];
        for (int j = 0; j < lengthJ; j++)
        {
            var sum = new GRBLinExpr();
            for (int i = 0; i < lengthI; i++)
            {
                if (!ReferenceEquals(variables[i, j], null))
                    sum.AddTerm(1, variables[i, j]);
            }

            result[j] = sum;
        }

        return result;
    }

    public static GRBLinExpr[] SumOverJ(this GRBVar?[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var result = new GRBLinExpr[lengthI];
        for (int i = 0; i < lengthI; i++)
        {
            var sum = new GRBLinExpr();
            for (int j = 0; j < lengthJ; j++)
            {
                if (!ReferenceEquals(variables[i, j], null))
                    sum.AddTerm(1, variables[i, j]);
            }

            result[i] = sum;
        }

        return result;
    }

    public static GRBLinExpr SumSum(this GRBVar?[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var sum = new GRBLinExpr();
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (!ReferenceEquals(variables[i, j], null))
                sum.AddTerm(1, variables[i, j]);
        }

        return sum;
    }

    public static GRBLinExpr[,] SumOverI(this GRBVar?[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new GRBLinExpr[countJ, countK];
        for (int j = 0; j < countJ; j++)
        for (int k = 0; k < countK; k++)
        {
            var sum = new GRBLinExpr();
            for (int i = 0; i < countI; i++)
            {
                if (!ReferenceEquals(variables[i, j, k], null))
                    sum.AddTerm(1, variables[i, j, k]);
            }

            result[j, k] = sum;
        }

        return result;
    }

    public static GRBLinExpr[,] SumOverJ(this GRBVar?[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new GRBLinExpr[countI, countK];
        for (int i = 0; i < countI; i++)
        for (int k = 0; k < countK; k++)
        {
            var sum = new GRBLinExpr();
            for (int j = 0; j < countJ; j++)
            {
                if (!ReferenceEquals(variables[i, j, k], null))
                    sum.AddTerm(1, variables[i, j, k]);
            }

            result[i, k] = sum;
        }

        return result;
    }

    public static GRBLinExpr[,] SumOverK(this GRBVar?[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new GRBLinExpr[countI, countJ];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            var sum = new GRBLinExpr();
            for (int k = 0; k < countK; k++)
            {
                if (!ReferenceEquals(variables[i, j, k], null))
                    sum.AddTerm(1, variables[i, j, k]);
            }

            result[i, j] = sum;
        }

        return result;
    }

    public static GRBLinExpr[] SumSumOverIJ(this GRBVar?[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new GRBLinExpr[countK];
        for (int k = 0; k < countK; k++)
        {
            var sum = new GRBLinExpr();
            for (int i = 0; i < countI; i++)
            for (int j = 0; j < countJ; j++)
            {
                if (!ReferenceEquals(variables[i, j, k], null))
                    sum.AddTerm(1, variables[i, j, k]);
            }

            result[k] = sum;
        }

        return result;
    }

    public static GRBLinExpr[] SumSumOverIK(this GRBVar?[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new GRBLinExpr[countJ];
        for (int j = 0; j < countJ; j++)
        {
            var sum = new GRBLinExpr();
            for (int i = 0; i < countI; i++)
            for (int k = 0; k < countK; k++)
            {
                if (!ReferenceEquals(variables[i, j, k], null))
                    sum.AddTerm(1, variables[i, j, k]);
            }

            result[j] = sum;
        }

        return result;
    }

    public static GRBLinExpr[] SumSumOverJK(this GRBVar?[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new GRBLinExpr[countI];
        for (int i = 0; i < countI; i++)
        {
            var sum = new GRBLinExpr();
            for (int j = 0; j < countJ; j++)
            for (int k = 0; k < countK; k++)
            {
                if (!ReferenceEquals(variables[i, j, k], null))
                    sum.AddTerm(1, variables[i, j, k]);
            }

            result[i] = sum;
        }

        return result;
    }
}