namespace Curupira.WindowsService.Model
{
    public class TaskModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string LastRunTime { get; set; }
        public string NextRunTime { get; set; }
    }
}