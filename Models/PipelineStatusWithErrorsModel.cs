namespace labelbox.Models
{
    public class PipelineStatusWithErrorsModel : PipelineStatusModel
    {
        public IEnumerable<dynamic> Errors { get; set; }
    }
}
