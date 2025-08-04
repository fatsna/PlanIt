using RunNow.Models;
using Newtonsoft.Json;
using System.IO;

namespace RunNow.Services
{
    public class ScenarioService
    {
        private readonly Dictionary<string, ScenarioStep> _steps;

        public ScenarioService()
        { 
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data","scenario.json");
            if (!File.Exists(path))
            {
                _steps = new Dictionary<string, ScenarioStep>();
                return;
            }

            var json = File.ReadAllText(path);
            _steps = JsonConvert.DeserializeObject<Dictionary<string, ScenarioStep>>(json) ?? new();
        }

        public ScenarioStep? GetStep(string id)
        {
            return _steps.ContainsKey(id) ? _steps[id] : null;
        }

        public string? MatchUserInput(ScenarioStep currentStep, string userInput)
        {
            if (currentStep.NextStep.ContainsKey(userInput))
                return currentStep.NextStep[userInput];
            return null;
        }

    }

}
