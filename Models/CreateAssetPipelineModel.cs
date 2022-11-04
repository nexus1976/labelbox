namespace labelbox.Models
{
    public class CreateAssetPipelineModel
    {
        public AssetPathModel AssetPath { get; set; }
        public PipelineWebhooksModel Notifications { get; set; }

        internal bool IsValid()
        {
            bool isValid = false;
            if (AssetPath == null || Notifications == null) return isValid;
            isValid = AssetPath.IsValid() && Notifications.IsValid();
            return isValid;
        }
    }
}
