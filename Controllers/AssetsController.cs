using labelbox.Data;
using labelbox.Models;
using labelbox.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace labelbox.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly ILogger<AssetsController> _logger;
        private readonly IExposedQueue _exposedQueue;
        private readonly IAssetService _assetService;
        private readonly IDataContext _dataContext;

        public AssetsController(ILogger<AssetsController> logger, IExposedQueue exposedQueue, IAssetService assetService, IDataContext dataContext)
        {
            _logger = logger;
            _exposedQueue = exposedQueue;
            _assetService = assetService;
            _dataContext = dataContext;
        }

        [HttpPost("image")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAsset([FromBody] CreateAssetPipelineModel model, CancellationToken cancellationToken)
        {
            try
            {
                if (model == null || !model.IsValid())
                    return await Task.FromResult(BadRequest());

                Asset asset = new()
                {
                    Id = Guid.NewGuid(),
                    Path = model.AssetPath.Path,
                    State = (int)PipelineStatusEnum.Queued,
                    OnStartURL = model.Notifications.OnStart,
                    OnSuccessURL = model.Notifications.OnSuccess,
                    OnFailureURL = model.Notifications.OnFailure
                };
                _dataContext.Assets.Add(asset);
                await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _exposedQueue.Queue.Add(asset.Id, cancellationToken);
                PipelineStatusModel response = new()
                {
                    Id = asset.Id.ToString(),
                    State = _assetService.ConvertEnumToString(PipelineStatusEnum.Queued)
                };
                return StatusCode(StatusCodes.Status202Accepted, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssetsController:PostAsset Exception occurred: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsset([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("id must be valid");

                var asset = await _dataContext.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken).ConfigureAwait(false);
                if (asset == null)
                    return NotFound();

                if (asset.State == PipelineStatusEnum.Failed)
                {
                    var response = new PipelineStatusWithErrorsModel()
                    {
                        Id = asset.Id.ToString(),
                        State = _assetService.ConvertEnumToString(PipelineStatusEnum.Failed),
                        Errors = _assetService.GetErrorsCollection(asset)
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new PipelineStatusModel()
                    {
                        Id = asset.Id.ToString(),
                        State = _assetService.ConvertEnumToString(asset.State)
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssetsController:GetAsset Exception occurred: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
