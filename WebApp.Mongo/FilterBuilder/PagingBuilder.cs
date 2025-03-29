using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApp.Mongo.FilterBuilder;

public static class PagingBuilder
{
    public static IFindFluent<T, TP> Paging<T, TP>(this IFindFluent<T, TP> source, int pageNumber, int pageSize)
    {
        return source.Skip((pageNumber - 1) * pageSize)
                     .Limit(pageSize);
    }
}