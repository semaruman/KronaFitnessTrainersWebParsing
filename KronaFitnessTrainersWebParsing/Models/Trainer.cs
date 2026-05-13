using System;
using System.Collections.Generic;
using System.Text;

namespace KronaFitnessTrainersWebParsing.Models
{
    public class Trainer
    {
        public string Name { get; set; }

        public string InfoHref { get; set; }

        public string Education {  get; set; }

        public List<string> Specializations { get; set; }

        public List<string> AdditionalEducations { get; set; }

        public int TrainingExperienceYears { get; set; }

        public int FitnessExperience {  get; set; }

        public bool IsCMS {  get; set; }

        public bool IsMS { get; set; }

        public string LinkToWorkout { get; set; }
    }
}
