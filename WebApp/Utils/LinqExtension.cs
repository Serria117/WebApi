namespace WebApp.Utils;

public static class LinqExtension
{
    /// <summary>
    /// Filters a sequence of values based on an asynchronous predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to filter.</param>
    /// <param name="predicate">An asynchronous function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token that observes cancellation requests.</param>
    /// <returns>An asynchronous enumerable that contains elements from the input sequence that satisfy the condition specified by the predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the source or predicate is null.</exception>
    public static async IAsyncEnumerable<TSource> WhereAsync<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<bool>> predicate,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        // Iterate through the synchronous source
        foreach (var item in source)
        {
            // Check for cancellation before awaiting the predicate
            cancellationToken.ThrowIfCancellationRequested();

            // Await the asynchronous predicate for each item
            bool includeItem = await predicate(item).ConfigureAwait(false);

            // If the predicate returned true, yield the item
            if (includeItem)
            {
                yield return item;
            }
        }
    }
}