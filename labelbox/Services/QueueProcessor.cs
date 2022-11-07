using labelbox.Data;
using Microsoft.EntityFrameworkCore;

namespace labelbox.Services
{
    public class QueueProcessor : IQueueProcessor
    {
        private readonly IAssetService _assetService;
        private readonly IDataContext _dataContext;
        private readonly IExposedQueue _queue;

        public QueueProcessor(IAssetService assetService, IDataContext dataContext, IExposedQueue queue)
        {
            _assetService = assetService;
            _dataContext = dataContext;
            _queue = queue;
        }

        public async Task ProcessMessageAsync(CancellationToken cancellationToken)
        {
            if (_queue != null)
            {
                bool queueProcessing = _queue.HasItemsInQueue();
                while (queueProcessing)
                {
                    var assetId = _queue.Dequeue(cancellationToken);
                    var asset = await _dataContext.Assets.FirstOrDefaultAsync(a => a.Id == assetId, cancellationToken).ConfigureAwait(false);
                    if (asset != null)
                    {
                        asset = _assetService.ValidateURLs(asset);
                        await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        if (asset.State != Models.PipelineStatusEnum.Failed)
                        {
                            _ = await _assetService.TrySendStartedEventAsync(asset, cancellationToken);
                            await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            if (asset.State != Models.PipelineStatusEnum.Failed)
                            {
                                _ = await _assetService.ValidateJPEG(asset, cancellationToken);
                                await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                                if (asset.State != Models.PipelineStatusEnum.Failed)
                                {
                                    _ = await _assetService.TrySendSuccessEventAsync(asset, cancellationToken);
                                    await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }

                        if (asset.State == Models.PipelineStatusEnum.Failed)
                        {
                            _ = await _assetService.TrySendFailureEventAsync(asset, cancellationToken);
                            await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        queueProcessing = false;
                    }
                    queueProcessing = _queue.HasItemsInQueue();
                }
            }
            return;
        }
    }
}
