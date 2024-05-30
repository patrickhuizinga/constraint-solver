namespace Solver.Lib;

public static class Extensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }
    
    public static bool IsEmpty<T>(this ICollection<T> source)
    {
        return source.Count == 0;
    }
    
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) 
            action(item);
    }
    
    public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
    {
        int i = 0;
        foreach (var item in source) 
            action(i++, item);
    }

    public static int Dim<T>(this T[] array) => array.Length;

    public static (int, int) Dim<T>(this T[,] array) => (array.LengthI(), array.LengthJ());

    public static (int, int, int) Dim<T>(this T[,,] array) => (array.LengthI(), array.LengthJ(), array.LengthK());

    public static int LengthI<T>(this T[,] array) => array.GetLength(0);
    public static int LengthJ<T>(this T[,] array) => array.GetLength(1);
    public static int LengthI<T>(this T[,,] array) => array.GetLength(0);
    public static int LengthJ<T>(this T[,,] array) => array.GetLength(1);
    public static int LengthK<T>(this T[,,] array) => array.GetLength(2);
    
    public static Expression Sum(this Variable[] variables)
    {
        return SumExpression.Create(variables);
    }
    
    public static Expression[] SumOverI(this Variable[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var result = new Expression[lengthJ];
        for (int j = 0; j < lengthJ; j++)
        {
            var parts = new Variable[lengthI];
            for (int i = 0; i < lengthI; i++)
                parts[i] = variables[i, j];

            result[j] = SumExpression.Create(parts);
        }

        return result;
    }

    public static Expression[] SumOverJ(this Variable[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var result = new Expression[lengthI];
        for (int i = 0; i < lengthI; i++)
        {
            var parts = new Variable[lengthJ];
            for (int j = 0; j < lengthJ; j++)
                parts[j] = variables[i, j];

            result[i] = SumExpression.Create(parts);
        }

        return result;
    }
    
    public static Expression[] SumOverI(this Expression[,] expressions)
    {
        var (lengthI, lengthJ) = expressions.Dim();

        var result = new Expression[lengthJ];
        for (int j = 0; j < lengthJ; j++)
        {
            var parts = new Expression[lengthI];
            for (int i = 0; i < lengthI; i++)
                parts[i] = expressions[i, j];

            result[j] = SumExpression.Create(parts);
        }

        return result;
    }

    public static Expression[] SumOverJ<TExpression>(this TExpression[,] expressions)
        where TExpression : Expression
    {
        var (lengthI, lengthJ) = expressions.Dim();

        var result = new Expression[lengthI];
        for (int i = 0; i < lengthI; i++)
        {
            var parts = new Expression[lengthJ];
            for (int j = 0; j < lengthJ; j++)
                parts[j] = expressions[i, j];

            result[i] = SumExpression.Create(parts);
        }

        return result;
    }

    public static Expression[,] SumOverI(this Variable[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new Expression[countJ, countK];
        for (int j = 0; j < countJ; j++)
        for (int k = 0; k < countK; k++)
        {
            var parts = new Variable[countI];
            for (int i = 0; i < countI; i++)
                parts[i] = variables[i, j, k];

            result[j, k] = SumExpression.Create(parts);
        }

        return result;
    }

    public static Expression[,] SumOverJ(this Variable[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new Expression[countI, countK];
        for (int i = 0; i < countI; i++)
        for (int k = 0; k < countK; k++)
        {
            var parts = new Variable[countJ];
            for (int j = 0; j < countJ; j++)
                parts[j] = variables[i, j, k];

            result[i, k] = SumExpression.Create(parts);
        }

        return result;
    }

    public static Expression[,] SumOverK(this Variable[,,] variables)
    {
        var (countI, countJ, countK) = variables.Dim();

        var result = new Expression[countI, countJ];
        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            var parts = new Variable[countK];
            for (int k = 0; k < countK; k++)
                parts[k] = variables[i, j, k];

            result[i, j] = SumExpression.Create(parts);
        }

        return result;
    }

    public static Expression[] SumSumOverIJ(this Variable[,,] variables)
    {
        return variables.SumOverJ().SumOverI();
    }

    public static Expression[] SumSumOverIK(this Variable[,,] variables)
    {
        return variables.SumOverK().SumOverI();
    }

    public static Expression[] SumSumOverJK(this Variable[,,] variables)
    {
        return variables.SumOverK().SumOverJ();
    }

    public static TExpression[] Diagonal<TExpression>(this TExpression[,] expressions)
        where TExpression : Expression
    {
        var lengthI = expressions.LengthI();

        var result = new TExpression[lengthI];
        for (int i = 0; i < lengthI; i++)
        {
            result[i] = expressions[i, i];
        }

        return result;
    }
}