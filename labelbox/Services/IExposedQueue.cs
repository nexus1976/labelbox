namespace labelbox.Services
{
    public interface IExposedQueue
    {
        void Enqueue(Guid item, CancellationToken cancellationToken);
    }
}
