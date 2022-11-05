using labelbox.Controllers;
using labelbox.Data;
using labelbox.Models;
using labelbox.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace labelbox.tests.Controllers
{
    [TestClass]
    public class AssetsControllerTests
    {
        private readonly AssetsController _controller;
        private readonly Mock<ILogger<AssetsController>> _loggerMock;
        private readonly Mock<IExposedQueue> _exposedQueueMock;
        private readonly Mock<IAssetService> _assetServiceMock;
        private readonly IDataContext _dataContext;
        private readonly FakeGuidQueue _fakeGuidQueue;

        public AssetsControllerTests()
        {
            _loggerMock = new Mock<ILogger<AssetsController>>();
            _exposedQueueMock = new Mock<IExposedQueue>();
            _assetServiceMock = new Mock<IAssetService>();
            _dataContext = new DataContext();
            _fakeGuidQueue = new();
            _exposedQueueMock.Setup(x => x.Enqueue(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Callback(_fakeGuidQueue.Add);

            _controller = new AssetsController(_loggerMock.Object, _exposedQueueMock.Object, _assetServiceMock.Object, _dataContext);
        }

        [TestMethod]
        public async Task WhenCalling_PostAsset_Returns202()
        {
            // Arrange
            _assetServiceMock.Setup(x => x.ConvertEnumToString(PipelineStatusEnum.Queued)).Returns("queued");
            var model = ArrangeUtility.GetCreateAssetPipelineModel();

            // Act
            var response = await _controller.PostAsset(model, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.ObjectResult;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(202, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }

        [TestMethod]
        public async Task WhenCalling_PostAsset_Returns400()
        {
            // Arrange
            _assetServiceMock.Setup(x => x.ConvertEnumToString(PipelineStatusEnum.Queued)).Returns("queued");
            var model = ArrangeUtility.GetCreateAssetPipelineModel();
            model.AssetPath.Path = String.Empty;

            // Act
            var response = await _controller.PostAsset(model, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [TestMethod]
        public async Task WhenCalling_PostAsset_Returns500()
        {
            // Arrange
            _assetServiceMock.Setup(x => x.ConvertEnumToString(PipelineStatusEnum.Queued)).Returns("queued");
            var model = ArrangeUtility.GetCreateAssetPipelineModel();
            ((DbContext)_dataContext).Dispose();

            // Act
            var response = await _controller.PostAsset(model, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }

        [TestMethod]
        public async Task WhenCalling_GetAsset_Returns200()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            _dataContext.Assets.Add(asset);
            await _dataContext.SaveChangesAsync();
            Guid id = asset.Id;

            // Act
            var response = await _controller.GetAsset(id, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.OkObjectResult;
            var resultValue = result?.Value as PipelineStatusModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(resultValue);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(id.ToString(), resultValue.Id);
        }

        [TestMethod]
        public async Task WhenCalling_GetAsset_Returns404()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            var response = await _controller.GetAsset(id, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [TestMethod]
        public async Task WhenCalling_GetAsset_Returns400()
        {
            // Arrange
            Guid id = Guid.Empty;

            // Act
            var response = await _controller.GetAsset(id, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [TestMethod]
        public async Task WhenCalling_GetAsset_Returns500()
        {
            // Arrange
            var asset = ArrangeUtility.GetSuccessAsset();
            _dataContext.Assets.Add(asset);
            await _dataContext.SaveChangesAsync();
            Guid id = asset.Id;
            ((DbContext)_dataContext).Dispose();

            // Act
            var response = await _controller.GetAsset(id, CancellationToken.None);
            var result = response as Microsoft.AspNetCore.Mvc.ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }
    }
}
