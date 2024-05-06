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

    public static (int, int) Dim<T>(this T[,] array) => (array.LengthI(), array.LengthJ());

    public static (int, int, int) Dim<T>(this T[,,] array) => (array.LengthI(), array.LengthJ(), array.LengthK());

    public static int LengthI<T>(this T[,] array) => array.GetLength(0);
    public static int LengthJ<T>(this T[,] array) => array.GetLength(1);
    public static int LengthI<T>(this T[,,] array) => array.GetLength(0);
    public static int LengthJ<T>(this T[,,] array) => array.GetLength(1);
    public static int LengthK<T>(this T[,,] array) => array.GetLength(2);
}