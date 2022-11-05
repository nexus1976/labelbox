using labelbox.Controllers;
using labelbox.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace labelbox.Services
{
    public class WorkerService : BackgroundService, IExposedQueue
    {
        private const int queuePollTime = 2000; // 2 seconds
        private readonly BlockingCollection<Guid> _queue;
        private readonly ILogger<WorkerService> _logger;
        private readonly IServiceScopeFactory _factory;

        public BlockingCollection<Guid> Queue { get { return _queue; } }
        public void Enqueue(Guid item, CancellationToken cancellationToken)
        {
            _queue.Add(item, cancellationToken);
        }

        public WorkerService(ILogger<WorkerService> logger, IServiceScopeFactory factory)
        {
            _logger = logger;
            _factory = factory;
            _queue = new BlockingCollection<Guid>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                    IDataContext dataContext = asyncScope.ServiceProvider.GetRequiredService<IDataContext>();
                    IAssetService assetService = asyncScope.ServiceProvider.GetRequiredService<IAssetService>();
                    await Task.Delay(queuePollTime, stoppingToken);
                    await DoQueuedWorkAsync(dataContext, assetService, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WorkerService:ExecuteAsync Exception occurred: {Message}", ex.Message);
                }
            }
            _logger.LogDebug("Exiting ExecuteAsync");
        }

        private async Task DoQueuedWorkAsync(IDataContext dataContext, IAssetService assetService, CancellationToken cancellationToken)
        {
            if (_queue != null)
            {
                bool queueProcessing = true;
                while (queueProcessing)
                {
                    var assetId = _queue.Take(cancellationToken);
                    var asset = await dataContext.Assets.FirstOrDefaultAsync(a => a.Id == assetId, cancellationToken).ConfigureAwait(false);
                    if (asset != null)
                    {
                        asset = assetService.ValidateURLs(asset);
                        await dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        if (asset.State != Models.PipelineStatusEnum.Failed)
                        {
                            _ = await assetService.TrySendStartedEventAsync(asset, cancellationToken);
                            await dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            if (asset.State != Models.PipelineStatusEnum.Failed)
                            {
                                _ = await assetService.ValidateJPEG(asset, cancellationToken);
                                await dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                                if (asset.State != Models.PipelineStatusEnum.Failed)
                                {
                                    _ = await assetService.TrySendSuccessEventAsync(asset, cancellationToken);
                                    await dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }

                        if (asset.State == Models.PipelineStatusEnum.Failed)
                        {
                            _ = await assetService.TrySendFailureEventAsync(asset, cancellationToken);
                            await dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        queueProcessing = false;
                    }
                }
            }
            return;
        }
    }
}
