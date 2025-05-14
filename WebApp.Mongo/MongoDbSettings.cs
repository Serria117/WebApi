using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Mongo;

/// <summary>
/// Represents the configuration settings required for connecting to a MongoDB instance.
/// This class encapsulates the connection string and the database name.
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}