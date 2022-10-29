using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using diskretni_mapd_simulace.Entities;

namespace diskretni_mapd_simulace.IO_Tools
{
    public static class PlanWriterIO
    {
        //TODO: take all agents, their plan and write them chronologically
        public static void writePlan(string fileName,Database db)
        {
            StreamWriter sw = new StreamWriter(fileName);

            //gather all plans
            List<Plan> plans = new List<Plan>();
            Plan currPlan = new Plan();


            foreach (Agent a in db.agents)
            {
                if (a.plan.steps.Count > 0) plans.Add(a.plan);
            }


            while (plans.Count > 0) 
            {
                int smallestTime = int.MaxValue;
                foreach (Plan p in plans)
                {
                    if (p.steps[0].time < smallestTime)
                    {
                        smallestTime = p.steps[0].time;
                        currPlan = p;
                    }
                }
                sw.WriteLine($"{currPlan.steps[0].time}-A-{currPlan.agent.id}-{currPlan.steps[0].locationId}");
                currPlan.steps.RemoveAt(0);
                if (currPlan.steps.Count <= 0) plans.Remove(currPlan);
            }
            sw.Close();
        }
    }
}
