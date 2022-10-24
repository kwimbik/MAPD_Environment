using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diskretni_mapd_simulace.Algorithms;
using diskretni_mapd_simulace.IO_Tools;

namespace diskretni_mapd_simulace
{
    public class PlanCreator
    {
        Database db;
        private Plan SimPlan;
        string algorithm;

        public PlanCreator(Database db, string algorithm)
        {
            this.db = db;
            this.algorithm = algorithm;
        }


        public void Solve()
        {
            Plan plan = new Plan();
            switch (algorithm)
            {   
                case "Greedy":
                    plan = GreedyAlg.run(db);
                    break;
            }

            this.SimPlan = plan;
            PlanWriterIO.writePlan(db.outputFile ,plan);
        }

    }


}
