namespace labelbox.Services
{
    public interface IExposedQueue
    {
        void Enqueue(Guid item, CancellationToken cancellationToken);
        Guid Dequeue(CancellationToken cancellationToken);
        bool HasItemsInQueue();
    }
}
