using labelbox.Data;
using labelbox.Models;
using labelbox.Services;

namespace labelbox.tests.Services
{
    [TestClass]
    public class QueueProcessorTests
    {
        private readonly Mock<IAssetService> _assetServiceMock;
        private readonly IExposedQueue _exposedQueue;
        private readonly IDataContext _dataContext;
        private readonly FakeGuidQueue _fakeGuidQueue;
        private readonly IQueueProcessor _queueProcessor;

        public QueueProcessorTests()
        {
            _dataContext = new DataContext();
            _fakeGuidQueue = new();
            _assetServiceMock = new Mock<IAssetService>();
            _exposedQueue = new ExposedQueue();
            _queueProcessor = new QueueProcessor(_assetServiceMock.Object, _dataContext, _exposedQueue);
        }

        [TestMethod]
        public async Task WhenCalling_ProcessMessageAsync_OneItem()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.State = PipelineStatusEnum.Queued;
            Guid expectedGUID = asset.Id;
            _dataContext.Assets.Add(asset);
            await _dataContext.SaveChangesAsync();
            _fakeGuidQueue.Add(expectedGUID, CancellationToken.None);
            _assetServiceMock.Setup(x => x.ValidateURLs(It.IsAny<Asset>())).Returns(asset);
            _assetServiceMock.Setup(x => x.TrySendStartedEventAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>())).ReturnsAsync(asset);
            _assetServiceMock.Setup(x => x.TrySendSuccessEventAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            {
                asset.State = PipelineStatusEnum.Success;
                return asset;
            });
            _assetServiceMock.Setup(x => x.ValidateJPEG(It.IsAny<Asset>(), It.IsAny<CancellationToken>())).ReturnsAsync(asset);

            // Act
            await _queueProcessor.ProcessMessageAsync(CancellationToken.None);
            await Task.Delay(2500);

            // Assert
            Assert.IsFalse(_exposedQueue.HasItemsInQueue());
            var persistedAsset = _dataContext.Assets.First();
            Assert.AreEqual(expectedGUID, persistedAsset.Id);
        }
    }
}
