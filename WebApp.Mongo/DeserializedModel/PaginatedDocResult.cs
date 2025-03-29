using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Mongo.DeserializedModel;
public class PaginatedDocResult
{
    public IList Data { get; set; }
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int PageCount { get; set; }
}


public class PaginatedDocResult<T> : PaginatedDocResult
{
    public new List<T> Data { get; set; } = [];
}