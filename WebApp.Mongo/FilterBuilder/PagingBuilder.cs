using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApp.Mongo.FilterBuilder;

public static class PagingBuilder
{
    /// <summary>
    /// Applies paging to the given collection by skipping a specified number of documents
    /// and limiting the number of results returned per page.
    /// </summary>
    /// <typeparam name="T">The type of the documents in the source collection.</typeparam>
    /// <typeparam name="TP">The type of the projection for the result documents.</typeparam>
    /// <param name="source">The source collection to apply paging to.</param>
    /// <param name="pageNumber">The number of the page to retrieve. Must be greater than 0.</param>
    /// <param name="pageSize">The number of documents to include in each page. Must be greater than 0.</param>
    /// <returns>An updated <see cref="IFindFluent{T, TP}"/> instance that includes the paging operations.</returns>
    public static IFindFluent<T, TP> Paging<T, TP>(this IFindFluent<T, TP> source, int pageNumber, int pageSize)
    {
        return source.Skip((pageNumber - 1) * pageSize)
                     .Limit(pageSize);
    }
}