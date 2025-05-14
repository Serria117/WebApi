namespace WebApp.Utils;

public static class LinqExtension
{
    public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        IEnumerable<T> enumerable = source as T[] ?? source.ToArray();
        var tasks = enumerable.Select(predicate).ToList();
        var results = await Task.WhenAll(tasks);
        return enumerable.Where((item, index) => results[index]).ToList();
    }
}