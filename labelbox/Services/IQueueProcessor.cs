namespace labelbox.Services
{
    public interface IQueueProcessor
    {
        Task ProcessMessageAsync(CancellationToken cancellationToken);
    }
}