namespace labelbox.Models
{
    public class PipelineWebhooksModel
    {
        public string OnStart { get; set; } = string.Empty;
        public string OnSuccess { get; set; } = string.Empty;
        public string OnFailure { get; set; } = string.Empty;

        internal bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(OnStart) || string.IsNullOrWhiteSpace(OnSuccess) || string.IsNullOrWhiteSpace(OnFailure))
                return false;

            return true;
        }
    }
}
