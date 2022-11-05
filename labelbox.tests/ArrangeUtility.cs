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
        internal static Asset GetFailedAsset()
        {
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                Path = "c:\\somefile.jpg",
                OnFailureURL = "notwellformed/url",
                OnFailureURLValidationError = "is not a valid URL",
                OnStartURL = "notwellformed/url",
                OnStartURLValidationError = "is not a valid URL",
                OnSuccessURL = "notwellformed/url",
                OnSuccessURLValidationError = "is not a valid URL",
                State = PipelineStatusEnum.Failed,
                AssetValidationError = "is not a jpeg"
            };
            return asset;
        }
        internal static MockFileSystem GetMockFileSystemWithJPGs()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\temp\endtable2.jpg", new MockFileData(GetEndTable2JPG()) },
                { @"C:\temp\endtable1.jpg", new MockFileData(GetEndTable1JPG()) },
                { @"C:\temp\endtable3.jpg", new MockFileData("this is not a jpg file...it lies!!") }
            });
            return fileSystem;
        }

        // jpg file that is 2016px x 1512px
        private static byte[] GetEndTable2JPG()
        {
            string path = "images/EndTable2.jpg";
            var bytes = File.ReadAllBytes(path);
            return bytes;
        }

        // jpg file that is 1000px x 750px
        private static byte[] GetEndTable1JPG()
        {
            string path = "images/EndTable1.jpg";
            var bytes = File.ReadAllBytes(path);
            return bytes;
        }
    }
}
