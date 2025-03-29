using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebApp.Core.DomainEntities;

public abstract class BaseEntity
{
    public bool Deleted { get; set; } = false;
}

public abstract class BaseEntity<TK> : BaseEntity
{
    [Key]
    public TK Id { get; set; }
}

public interface IBaseEntityAuditable
{
    public DateTime CreateAt { get; set; }
    public DateTime LastUpdateAt { get; set; }
    public string? CreateBy { get; set; }
}


public abstract class BaseEntityAuditable<TK> : BaseEntity<TK>, IBaseEntityAuditable
{
    public DateTime CreateAt { get; set; }
    public DateTime LastUpdateAt { get; set; }
    public string? CreateBy { get; set; }
}