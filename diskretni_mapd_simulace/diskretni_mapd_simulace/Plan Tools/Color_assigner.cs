using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace diskretni_mapd_simulace.Plan_Tools
{
    public class Color_assigner
    {
        Database db;
        Routing_solverManager rsm;
        RoutingSolverResults results;
        byte[][] colors = new byte[6][] {
                 new byte[]{ 255, 0, 0 },
                 new byte[] { 255, 165, 0 },
                 new byte[]{ 0, 0, 255 },
                 new byte[]{ 255, 0, 165 },
                 new byte[]{ 0, 255, 165 },
                 new byte[] { 165, 0, 255 }};

        public Color_assigner(Database db)
        {
            this.db = db;
        }


        public void assignColors(RoutingSolverResults rsr)
        {
            Agent a;
            List<Order> loadedOrders = new List<Order>();
            for (int i = 0; i < rsr.routingSolverManager.AgentNumber; ++i)
            {
                a = rsr.routingSolverManager.indexToAgentMap[i];
                a.orders.Clear();
                var index = rsr.routingModel.Start(i);
                while (rsr.routingModel.IsEnd(index) == false)
                {
                    Location l = rsr.routingSolverManager.indexToLocationMap[rsr.routingIndexManager.IndexToNode(index)];
                    index = rsr.solution.Value(rsr.routingModel.NextVar(index));
                    foreach (var o in l.orders)
                    {
                        if (loadedOrders.Contains(o) == false)
                        {
                            a.orders.Add(o);
                            loadedOrders.Add(o);
                        }
                    }
                }
            }

            //TODO: generate colors based on number of agents, viz prepared fction obtainColorRange
            //for onw, agent color limit is 7
            int agent_counter = 0;
            foreach (var agent in db.agents)
            {
                agent.color = colors[agent_counter];
                foreach (var order in agent.orders)
                {
                    order.color = colors[agent_counter];
                }
                agent_counter++;
            }
        }

        public void assignOrders()
        {
            Color_assigner ca = new Color_assigner(db);
            //TSP task
            Task TSP = new Task(runTSP);

            //Color assign task
            Task colorAssign = new Task(() => ca.assignColors(results));

            //Visualize Colors tasl
            //Task visual = new Task(sv.colorAssignments);

            TSP.Start();
            TSP.Wait();
            colorAssign.Start();
            colorAssign.Wait();

            //visual.Start();
        }

        private void runTSP()
        {
            Routing_solverManager rsm = new Routing_solverManager(db);
            rsm.getSolutionData();
            if (rsm.ordersToProcess.Count == 0)
            {
                Console.WriteLine("No orders to plan");
            }
            else
            {
                Routing_solver rs = new Routing_solver(rsm);
                results = rs.solveProblemAndPrintResults();
            }
        }

        private List<byte[]> obtainColorRange(int num_of_colots)
        {
            return null;
        }
    }
}
