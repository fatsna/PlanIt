namespace RunNow.Models
{
    public class ScenarioStep
    {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public Dictionary<string, string> NextStep { get; set; } = new();
    }
}
