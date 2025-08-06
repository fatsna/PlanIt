using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RunNow
{
    public class CareerProfile
    {
        public string CurrentPosition { get; set; } = "";
        public int YearsOfExperience { get; set; } = 0;
        public List<string> SkillsAndCerts { get; set; } = new();
        public List<string> InterestedIndustries { get; set; } = new();
    }
}
