namespace Solver.Lib;

public static class Combinations
{
    private static Dictionary<(int n, int k), int> Cache = new Dictionary<(int n, int k), int>();

    public static int Get(int n, int k)
    {
        if (2 * k < n)
            k = n - k;

        if (Cache.TryGetValue((n, k), out var result))
            return result;

        result = CalcCombinations(n, k);
        Cache[(n, k)] = result;
        return result;
    }

    private static int CalcCombinations(int n, int k)
    {
        var nk = n - k;

        int result = 1;
        for (int i = k + 1; i <= n; i++)
        {
            if (nk > 0 && i % nk == 0)
            {
                result *= i / nk;
                nk--;
            }
            else
            {
                result *= i;
            }
        }

        for (int i = 1; i <= nk; i++) 
            result /= i;

        return result;
    }
}