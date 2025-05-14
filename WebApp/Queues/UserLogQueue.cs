using System.Collections.Concurrent;
using WebApp.Core.DomainEntities;

namespace WebApp.Queues;

public interface IUserLogQueue
{
    void Enqueue(UserLog log);
    bool TryDequeue(out UserLog log);
    int Count { get; }
}

public class UserLogQueue : IUserLogQueue
{
    private readonly ConcurrentQueue<UserLog> _queue = new();

    public void Enqueue(UserLog log)
    {
        _queue.Enqueue(log);
    }

    public bool TryDequeue(out UserLog log)
    {
        return _queue.TryDequeue(out log);
    }

    public int Count => _queue.Count;
}
