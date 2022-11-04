using labelbox.Models;

namespace labelbox.Data
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string OnStartURL { get; set; }
        public string OnSuccessURL { get; set; }
        public string OnFailureURL { get; set; }
        public PipelineStatusEnum State { get; set; }
        public string AssetValidationError { get; set; }
        public string OnStartURLValidationError { get; set; }
        public string OnSuccessURLValidationError { get; set; }
        public string OnFailureURLValidationError { get; set; }
    }
}
