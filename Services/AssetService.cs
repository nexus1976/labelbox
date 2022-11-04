using labelbox.Data;
using labelbox.Models;
using SkiaSharp;
using System.Drawing;
using System.Dynamic;
using System.Text.Json;

namespace labelbox.Services
{
    public class AssetService : IAssetService
    {
        public Asset ValidateURLs(Asset asset)
        {
            if (asset != null)
            {
                if (!Uri.IsWellFormedUriString(asset.OnStartURL, UriKind.Absolute))
                {
                    asset.OnStartURLValidationError = "is not a valid URL";
                    asset.State = PipelineStatusEnum.Failed;
                }
                if (!Uri.IsWellFormedUriString(asset.OnSuccessURL, UriKind.Absolute))
                {
                    asset.OnSuccessURLValidationError = "is not a valid URL";
                    asset.State = PipelineStatusEnum.Failed;
                }
                if (!Uri.IsWellFormedUriString(asset.OnFailureURL, UriKind.Absolute))
                {
                    asset.OnFailureURLValidationError = "is not a valid URL";
                    asset.State = PipelineStatusEnum.Failed;
                }
            }
            return asset;
        }

        public async Task<Asset> TrySendStartedEventAsync(Asset asset, CancellationToken cancellationToken)
        {
            if (asset != null)
            {
                PipelineStatusEnum state = PipelineStatusEnum.Started;
                using HttpClient httpClient = new();
                var status = new PipelineStatusModel()
                {
                    Id = asset.Id.ToString(),
                    State = ConvertEnumToString(state)
                };
                var jsonOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string jsonString = JsonSerializer.Serialize(status, jsonOptions);
                using var httpContent = new StringContent(jsonString);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                bool postSuccess = true;
                try
                {
                    using var response = await httpClient.PostAsync(asset.OnStartURL, httpContent, cancellationToken);
                    postSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    postSuccess = false;
                }
                
                if (!postSuccess)
                {
                    asset.State = PipelineStatusEnum.Failed;
                    asset.OnStartURLValidationError = "is not a valid URL";
                }
                else
                {
                    asset.State = PipelineStatusEnum.InProgress;
                }
            }
            return asset;
        }

        public async Task<Asset> TrySendSuccessEventAsync(Asset asset, CancellationToken cancellationToken)
        {
            if (asset != null)
            {
                PipelineStatusEnum state = PipelineStatusEnum.Success;
                using HttpClient httpClient = new();
                var status = new PipelineStatusModel()
                {
                    Id = asset.Id.ToString(),
                    State = ConvertEnumToString(state)
                };
                var jsonOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string jsonString = JsonSerializer.Serialize(status, jsonOptions);
                using var httpContent = new StringContent(jsonString);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                bool postSuccess = true;
                try
                {
                    using var response = await httpClient.PostAsync(asset.OnSuccessURL, httpContent, cancellationToken);
                    postSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    postSuccess = false;
                }

                if (!postSuccess)
                {
                    asset.State = PipelineStatusEnum.Failed;
                    asset.OnSuccessURLValidationError = "is not a valid URL";
                }
                else
                {
                    asset.State = PipelineStatusEnum.Complete;
                }
            }
            return asset;
        }

        public async Task<Asset> TrySendFailureEventAsync(Asset asset, CancellationToken cancellationToken)
        {
            if (asset != null)
            {
                PipelineStatusEnum state = PipelineStatusEnum.Failed;
                using HttpClient httpClient = new();
                var status = new PipelineStatusWithErrorsModel()
                {
                    Id = asset.Id.ToString(),
                    State = ConvertEnumToString(state),
                    Errors = GetErrorsCollection(asset)
                };
                var jsonOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string jsonString = JsonSerializer.Serialize(status, jsonOptions);
                using var httpContent = new StringContent(jsonString);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                bool postSuccess = true;
                try
                {
                    using var response = await httpClient.PostAsync(asset.OnFailureURL, httpContent, cancellationToken);
                    postSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    postSuccess = false;
                }

                if (!postSuccess)
                {
                    asset.State = PipelineStatusEnum.Failed;
                    asset.OnFailureURLValidationError = "is not a valid URL";
                }
                else
                {
                    asset.State = PipelineStatusEnum.Failed;
                }
            }
            return asset;
        }

        public IEnumerable<dynamic> GetErrorsCollection(Asset asset)
        {
            var errors = new List<dynamic>();
            if (asset != null)
            {
                if (!string.IsNullOrWhiteSpace(asset.OnStartURLValidationError))
                    errors.Add(new OnStartErrorModel() { OnStart = asset.OnStartURLValidationError });
                if (!string.IsNullOrWhiteSpace(asset.OnSuccessURLValidationError))
                    errors.Add(new OnSuccessErrorModel() { OnSuccess = asset.OnSuccessURLValidationError });
                if (!string.IsNullOrWhiteSpace(asset.OnFailureURLValidationError))
                    errors.Add(new OnFailureErrorModel() { OnFailure = asset.OnFailureURLValidationError });
                if (!string.IsNullOrWhiteSpace(asset.AssetValidationError))
                    errors.Add(new AssetErrorModel() { Asset = asset.AssetValidationError });
            }
            return errors;
        }
        public string ConvertEnumToString(PipelineStatusEnum pipelineStatusEnum)
        {
            return pipelineStatusEnum switch
            {
                PipelineStatusEnum.Queued => "queued",
                PipelineStatusEnum.Started => "started",
                PipelineStatusEnum.InProgress => "in_progress",
                PipelineStatusEnum.Success => "success",
                PipelineStatusEnum.Complete => "complete",
                PipelineStatusEnum.Failed => "failed",
                _ => "unknown",
            };
        }

        public async Task<Asset> ValidateJPEG(Asset asset, CancellationToken cancellationToken)
        {
            try
            {
                if (File.Exists(asset.Path))
                {
                    var fileBytes = File.ReadAllBytes(asset.Path);
                    if (IsJPG(fileBytes))
                    {
                        using MemoryStream memoryStream = new(fileBytes);
                        using SKBitmap bitmap = SKBitmap.Decode(memoryStream);
                        if (bitmap.Height <= 1000 || bitmap.Width <= 1000)
                        {
                            asset.State = PipelineStatusEnum.Failed;
                            asset.AssetValidationError = "jpeg does not have a width and/or height greater than 1000px";
                        }
                    }
                    else
                    {
                        asset.State = PipelineStatusEnum.Failed;
                        asset.AssetValidationError = "is not a jpeg";
                    }
                }
                else
                {
                    asset.State = PipelineStatusEnum.Failed;
                    asset.AssetValidationError = "is not reachable by the server";
                }
            }
            catch (Exception)
            {
                asset.State = PipelineStatusEnum.Failed;
                asset.AssetValidationError = "is not reachable by the server";
            }
            return await Task.FromResult(asset);
        }

        private static bool IsJPG(byte[] fileBytes)
        {
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (jpeg.SequenceEqual(fileBytes.Take(jpeg.Length)))
                return true;
            else if (jpeg2.SequenceEqual(fileBytes.Take(jpeg2.Length)))
                return true;
            else
                return false;
        }
    }
}
