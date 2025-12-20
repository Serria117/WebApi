using System.Linq.Expressions;

namespace WebApp.Utils;

public static class LinqExtension
{
    /// <summary>
    /// Asynchronously filters an enumerable sequence based on a specified predicate function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The source IEnumerable sequence to which the filter is applied.</param>
    /// <param name="predicate">An asynchronous function to test each element for a condition.</param>
    /// <returns>
    /// A task representing the asynchronous operation that returns an IEnumerable sequence containing elements
    /// that satisfy the predicate function.
    /// </returns>
    public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        IEnumerable<T> enumerable = source as T[] ?? source.ToArray();
        var tasks = enumerable.Select(predicate).ToList();
        var results = await Task.WhenAll(tasks);
        return enumerable.Where((item, index) => results[index]).ToList();
    }

    /// <summary>
    /// Applies a conditional filter to an IQueryable sequence based on a specified condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The source IQueryable sequence to which the filter is applied.</param>
    /// <param name="condition">A boolean condition that determines whether the filter is applied.</param>
    /// <param name="predicate">The predicate expression used as the filter when the condition is true.</param>
    /// <returns>
    /// An IQueryable sequence with the filter applied if the condition is true;
    /// otherwise, the original source sequence without the filter.
    /// </returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition,
                                           Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}