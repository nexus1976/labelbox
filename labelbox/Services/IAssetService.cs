using labelbox.Data;
using labelbox.Models;

namespace labelbox.Services
{
    public interface IAssetService
    {
        Task<Asset> TrySendFailureEventAsync(Asset asset, CancellationToken cancellationToken);
        Task<Asset> TrySendStartedEventAsync(Asset asset, CancellationToken cancellationToken);
        Task<Asset> TrySendSuccessEventAsync(Asset asset, CancellationToken cancellationToken);
        Task<Asset> ValidateJPEG(Asset asset, CancellationToken cancellationToken);
        Asset ValidateURLs(Asset asset);
        IEnumerable<dynamic> GetErrorsCollection(Asset asset);
        string ConvertEnumToString(PipelineStatusEnum pipelineStatusEnum);
    }
}