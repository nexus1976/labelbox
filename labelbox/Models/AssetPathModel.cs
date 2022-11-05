namespace labelbox.Models
{
    public class AssetPathModel
    {
        public string Location { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;

        internal bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Location) || Location.ToLowerInvariant().Trim() != "local" || string.IsNullOrWhiteSpace(Path)) return false;

            return true;
        }
    }
}
