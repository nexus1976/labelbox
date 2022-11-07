namespace labelbox.Services
{
    public class WorkerService : BackgroundService
    {
        private const int queuePollTime = 2000; // 2 seconds
        private readonly ILogger<WorkerService> _logger;
        private readonly IServiceScopeFactory _factory;

        public WorkerService(ILogger<WorkerService> logger, IServiceScopeFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                    IQueueProcessor queueProcessor = asyncScope.ServiceProvider.GetRequiredService<IQueueProcessor>();
                    await queueProcessor.ProcessMessageAsync(stoppingToken);
                    await Task.Delay(queuePollTime, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WorkerService:ExecuteAsync Exception occurred: {Message}", ex.Message);
                }
            }
            _logger.LogDebug("Exiting ExecuteAsync");
        }
    }
}
