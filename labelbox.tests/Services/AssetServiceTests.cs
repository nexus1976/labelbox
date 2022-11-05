using labelbox.Models;
using labelbox.Services;
using Moq.Protected;

namespace labelbox.tests.Services
{
    [TestClass]
    public class AssetServiceTests
    {
        private readonly IAssetService _assetService;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        public AssetServiceTests()
        {
            var fileSystemMock = ArrangeUtility.GetMockFileSystemWithJPGs();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _assetService = new AssetService(fileSystemMock, _httpClientFactoryMock.Object);
        }

        [TestMethod]
        public void WhenCalling_ValidateURLs_AllGoodUrls()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();

            // Act
            var result = _assetService.ValidateURLs(asset);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Success, result.State);
        }

        [TestMethod]
        public void WhenCalling_ValidateURLs_BadOnSuccessUrl()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.OnSuccessURL = "notwellformed/url";

            // Act
            var result = _assetService.ValidateURLs(asset);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
            Assert.IsNotNull(asset.OnSuccessURLValidationError);
        }

        [TestMethod]
        public void WhenCalling_ValidateURLs_BadOnStartUrl()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.OnStartURL = "notwellformed/url";

            // Act
            var result = _assetService.ValidateURLs(asset);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
            Assert.IsNotNull(asset.OnStartURLValidationError);
        }

        [TestMethod]
        public void WhenCalling_ValidateURLs_BadOnFailureUrl()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.OnFailureURL = "notwellformed/url";

            // Act
            var result = _assetService.ValidateURLs(asset);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
            Assert.IsNotNull(asset.OnFailureURLValidationError);
        }

        [TestMethod]
        public void WhenCalling_GetErrorsCollection_WithAllFailures()
        {
            // Arrange
            var asset = ArrangeUtility.GetFailedAsset();

            // Act
            var result = _assetService.GetErrorsCollection(asset);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() == 4);
        }

        [TestMethod]
        public void WhenCalling_GetErrorsCollection_WithNoFailures()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();

            // Act
            var result = _assetService.GetErrorsCollection(asset);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void WhenCalling_ConvertEnumToString_AllValues()
        {
            // Arrange
            var queuedEnum = PipelineStatusEnum.Queued;
            var startedEnum = PipelineStatusEnum.Started;
            var inProgressEnum = PipelineStatusEnum.InProgress;
            var successEnum = PipelineStatusEnum.Success;
            var completeEnum = PipelineStatusEnum.Complete;
            var failedEnum = PipelineStatusEnum.Failed;

            // Act
            var queuedString = _assetService.ConvertEnumToString(queuedEnum);
            var startedString =  _assetService.ConvertEnumToString(startedEnum);
            var inProgressString = _assetService.ConvertEnumToString(inProgressEnum);
            var successString = _assetService.ConvertEnumToString(successEnum);
            var completeString = _assetService.ConvertEnumToString(completeEnum);
            var failedString = _assetService.ConvertEnumToString(failedEnum);

            // Assert
            Assert.AreEqual("queued", queuedString);
            Assert.AreEqual("started", startedString);
            Assert.AreEqual("in_progress", inProgressString);
            Assert.AreEqual("success", successString);
            Assert.AreEqual("complete", completeString);
            Assert.AreEqual("failed", failedString);
        }

        [TestMethod]
        public async Task WhenCalling_ValidateJPEG_WithGoodJPG()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.Path = "C:\\temp\\endtable1.jpg";

            // Act
            var result = await _assetService.ValidateJPEG(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Success, result.State);
        }

        [TestMethod]
        public async Task WhenCalling_ValidateJPEG_WithTooSmallJPG()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.Path = "C:\\temp\\endtable2.jpg";

            // Act
            var result = await _assetService.ValidateJPEG(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AssetValidationError);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
            Assert.AreEqual("jpeg does not have a width and/or height greater than 1000px", result.AssetValidationError);
        }

        [TestMethod]
        public async Task WhenCalling_ValidateJPEG_WithNotAJPG()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.Path = "C:\\temp\\endtable3.jpg";

            // Act
            var result = await _assetService.ValidateJPEG(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AssetValidationError);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
            Assert.AreEqual("is not a jpeg", result.AssetValidationError);
        }

        [TestMethod]
        public async Task WhenCalling_ValidateJPEG_WithAnUnreachableFile()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            asset.Path = "C:\\pictures\\endtable3.jpg";

            // Act
            var result = await _assetService.ValidateJPEG(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AssetValidationError);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
            Assert.AreEqual("is not reachable by the server", result.AssetValidationError);
        }

        [TestMethod]
        public async Task WhenCalling_TrySendStartedEventAsync_WithGoodURL()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            var responseMock = new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = System.Net.HttpStatusCode.OK
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMock);
            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty)).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            // Act
            var result = await _assetService.TrySendStartedEventAsync(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.InProgress, result.State); 
        }

        [TestMethod]
        public async Task WhenCalling_TrySendStartedEventAsync_WithBadURL()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            var responseMock = new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = System.Net.HttpStatusCode.Unauthorized
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMock);
            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty)).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            // Act
            var result = await _assetService.TrySendStartedEventAsync(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
        }

        [TestMethod]
        public async Task WhenCalling_TrySendSuccessEventAsync_WithGoodURL()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            var responseMock = new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = System.Net.HttpStatusCode.OK
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMock);
            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty)).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            // Act
            var result = await _assetService.TrySendSuccessEventAsync(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Complete, result.State);
        }

        [TestMethod]
        public async Task WhenCalling_TrySendSuccessEventAsync_WithBadURL()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            var responseMock = new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = System.Net.HttpStatusCode.Unauthorized
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMock);
            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty)).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            // Act
            var result = await _assetService.TrySendSuccessEventAsync(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
        }

        [TestMethod]
        public async Task WhenCalling_TrySendFailureEventAsync_WithGoodURL()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            var responseMock = new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = System.Net.HttpStatusCode.OK
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMock);
            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty)).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            // Act
            var result = await _assetService.TrySendFailureEventAsync(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
        }

        [TestMethod]
        public async Task WhenCalling_TrySendFailureEventAsync_WithBadURL()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            var responseMock = new HttpResponseMessage
            {
                Content = new StringContent(string.Empty),
                StatusCode = System.Net.HttpStatusCode.Unauthorized
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMock);
            _httpClientFactoryMock.Setup(x => x.CreateClient(string.Empty)).Returns(new HttpClient(_httpMessageHandlerMock.Object));

            // Act
            var result = await _assetService.TrySendFailureEventAsync(asset, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(PipelineStatusEnum.Failed, result.State);
        }
    }
}
