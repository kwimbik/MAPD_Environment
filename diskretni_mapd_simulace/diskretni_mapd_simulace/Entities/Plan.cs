using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diskretni_mapd_simulace.Entities;

namespace diskretni_mapd_simulace
{
    public class Plan
    {
        public string value;
        //TODO: rather than value in string, I will hold for each agent Plan, consisting of PlanSteps
        public List<PlanStep> steps = new List<PlanStep>();
    }
}
