using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using diskretni_mapd_simulace.Algorithms;
using diskretni_mapd_simulace.Entities;
using diskretni_mapd_simulace.IO_Tools;
using System.Timers;
using System.Diagnostics;
using diskretni_mapd_simulace.Plan_Tools;

namespace diskretni_mapd_simulace
{
    public class PlanCreator
    {
        Database db;
        private Plan simulationPlan;
        string algorithm;


        public int settings; // 0 stress test, 1 for set number of offers, 2 for not outputing plans, 3 for bt test - increasing agents


        private int firstKOrders = -1;
        private bool showPlan = true;
        public int currentRun = 1;
        public int secondsForStressTestTimeOut = 0;

        public PlanCreator(Database db)
        {
            this.db = db;
        }

        public void setShowPlan(bool showPlan)
        {
            this.showPlan = showPlan;
        }

        
        //Once solver is ready, pass number of scenarios to solve / stress test testtin to the PlanCreator
        public void setSettings(int settingOfOrders, int firstKOrders)
        {
            if (settingOfOrders != 0 && settingOfOrders != 1 && settingOfOrders != 2 && settingOfOrders != 3)
            {
                throw new ArgumentException("Wrong settings parametr");
            }
            settings = settingOfOrders;
            this.firstKOrders = firstKOrders;
        }

        private int getNumberOfOrdersToProcess()
        {
            if (settings == 1 || settings == 3) return firstKOrders;
            else return currentRun; //This will be incremented for stress test after each succesfull creation of a plan within time limit
        }

        public bool LoadScenario()
        {
            int numberOfOrdersToProcess = getNumberOfOrdersToProcess();

            return db.LoadScenario(numberOfOrdersToProcess);
        }

        private ProblemObject createProblemObject()
        {
            ProblemObject po = new ProblemObject(db.orders, db.agents, db.locations, db.locationMap);
            List<Location> initLocations = new List<Location>();
            List<Location> targetLocations = new List<Location>();
            foreach (var o in po.orders)
            {
                var init = o.currLocation;
                var target = o.targetLocation;
                if (initLocations.Contains(init) || targetLocations.Contains(target)) throw new Exception($"Assert, doplicate objednavky: \n {o.currLocation.coordination[0]},{o.currLocation.coordination[1]}");
                initLocations.Add(init);
                targetLocations.Add(target);
            }

            return po;
        }


        public Plan Solve()
        {
            //gets selected Algorithm
            //Not database, ale PlanCreator ma tohle drzet
            algorithm = db.selectedAlgo;

            Plan plan = new Plan();

            //measures time for plan finding
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            ProblemObject po = createProblemObject();


            switch (algorithm)
            {
                case "Greedy":
                    Color_assigner cs = new Color_assigner(db);
                    cs.assignOrders();
                    plan = GreedyAlg.run(db);
                    break;
                case "TP":
                    plan = TokenPassingAlg.run(po);
                    break;
                case "TPTS":
                    plan = TokenPassingTasksSwapingAlg.run(po);
                    break;
                case "CENTRAL_ASTAR":
                    plan = CentralAlg_ASTAR.run(po);
                    break;
                case "CENTRAL_CBS":
                    plan = CentralAlg_CBS.run(po);
                    break;
                default:
                    MessageBox.Show("Invalid algorithm");
                    return plan;
            }

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
           
            this.simulationPlan = plan;
            string outputFIle = db.outputFile;


            //komplet vyhodit, nevolat write plan odsud, jen ho vracet
            if (settings == 0)
            {
                //writes plan with sequence number
                PlanWriterIO.writePlan(outputFIle.Split(".")[0] + $" - {currentRun}.plan", simulationPlan);
                //TODO: ping stw, update new best, how many steps + time
            }
            else if (settings == 1)
            {
                PlanWriterIO.writePlan(outputFIle, simulationPlan);
            }
            
            SolutionPacket sp = new SolutionPacket(simulationPlan.steps.Count, ts, firstKOrders, po.agents.Count, algorithm, db.mapName,db.scenarioName, simulationPlan.steps[simulationPlan.steps.Count-1].time, simulationPlan.serviceTime);
            plan.solutionPacket = sp;
            plan.mapName = db.mapName;
            //MessageBox.Show($"Plan steps: {plan.solutionPacket.number_of_steps}\n Time:{plan.solutionPacket.run_time}");

            return plan;
        }
    }
}
