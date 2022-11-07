using System.Collections.Concurrent;

namespace labelbox.Services
{
    public class ExposedQueue : IExposedQueue
    {
        private readonly BlockingCollection<Guid> _queue;

        public ExposedQueue()
        {
            _queue = new BlockingCollection<Guid>();
        }

        public void Enqueue(Guid item, CancellationToken cancellationToken)
        {
            _queue.Add(item, cancellationToken);
        }
        public Guid Dequeue(CancellationToken cancellationToken)
        {
            return _queue.Take(cancellationToken);
        }
        public bool HasItemsInQueue()
        {
            return _queue.Any();
        }
    }
}
