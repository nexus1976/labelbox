using System.Collections.Concurrent;

namespace labelbox.Services
{
    public interface IExposedQueue
    {
        public BlockingCollection<Guid> Queue { get; }
    }
}
