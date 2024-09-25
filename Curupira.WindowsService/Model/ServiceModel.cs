namespace Curupira.WindowsService.Model
{
    public class ServiceModel
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string StartType { get; set; }
        public string ServiceAccount { get; set; }
    }
}
