using labelbox.Data;
using labelbox.Models;

namespace labelbox.tests
{
    internal class ArrangeUtility
    {
        internal static CreateAssetPipelineModel GetCreateAssetPipelineModel()
        {
            var model = new CreateAssetPipelineModel()
            {
                AssetPath = new AssetPathModel()
                {
                    Location = "local",
                    Path = "c:\\somefile.jpg"
                },
                Notifications = new PipelineWebhooksModel()
                {
                    OnFailure = "http://fakeendpoint",
                    OnStart = "http://fakeendpoint",
                    OnSuccess = "http://fakeendpoint"
                }
            };
            return model;
        }
        internal static Asset GetSuccessAsset()
        {
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                Path = "c:\\somefile.jpg",
                OnFailureURL = "http://fakeendpoint",
                OnStartURL = "http://fakeendpoint",
                OnSuccessURL = "http://fakeendpoint",
                State = PipelineStatusEnum.Success
            };
            return asset;
        }
    }
}
