using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Plan_Tools
{
    public class Color_assigner
    {
        Database db;
        byte[][] colors = new byte[7][] {
                 new byte[]{ 255, 0, 0 },
                 new byte[]{ 0, 255, 0 },
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
            //TODO: locations where agent is supposed to go, not necessearly orders -> find a way to get orders for each agent
            Agent a;
            for (int i = 0; i < rsr.routingSolverManager.AgentNumber; ++i)
            {
                List<Order> orders = new List<Order>();
                a = rsr.routingSolverManager.indexToAgentMap[i];

                var index = rsr.routingModel.Start(i);
                while (rsr.routingModel.IsEnd(index) == false)
                {
                    Location l = rsr.routingSolverManager.indexToLocationMap[rsr.routingIndexManager.IndexToNode(index)];
                    index = rsr.solution.Value(rsr.routingModel.NextVar(index));
                    foreach (var o in l.orders)
                    {
                        orders.Add(o);
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

        private List<byte[]> obtainColorRange(int num_of_colots)
        {
            return null;
        }
    }
}
