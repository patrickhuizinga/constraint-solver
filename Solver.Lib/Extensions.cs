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
}