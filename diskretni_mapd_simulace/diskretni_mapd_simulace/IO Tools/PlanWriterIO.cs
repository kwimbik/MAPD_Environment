using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using diskretni_mapd_simulace.Entities;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Windows.Controls;
using System.Windows;

namespace diskretni_mapd_simulace.IO_Tools
{
    public static class PlanWriterIO
    {       
        private static void writeAgents(StreamWriter sw, Plan plan)
        {
            foreach (var agent in plan.agents)
            {
                sw.WriteLine($"A-{agent.id}-{agent.baseLocation.id}");
            }
        }


        private static void writeOrders(StreamWriter sw, Plan plan)
        {
            foreach (var order in plan.orders.OrderBy(x=> int.Parse(x.id)))
            {
                sw.WriteLine($"O-{order.id}-{order.currLocation.id}-{order.targetLocation.id}");
            }
        }

        private static void writeAgentOrderAssignments(StreamWriter sw, Plan plan)
        {
            foreach (Agent a in plan.agents)
            {
                if (plan.agentOrderDict[a].Count == 0) continue;
                string orders = "";
                for (int i = 0; i < plan.agentOrderDict[a].Count; i++)
                {
                    orders += plan.agentOrderDict[a][i].id;
                    if (i != plan.agentOrderDict[a].Count - 1) orders += ",";
                }
               
                sw.WriteLine($"AS-{a.id}-{orders}");
            }
        }


        public static void writePlan(string fileName, Plan plan)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(fileName);
            }
            catch
            {
                MessageBox.Show("File is open");
                return;
            }
            

            writeAgents(sw, plan);
            writeOrders(sw, plan);
            writeAgentOrderAssignments(sw, plan); 

            foreach (PlanStep ps in plan.steps)
            {
                if (ps.type == (int)PlanStep.types.movement)
                {
                    sw.WriteLine($"{ps.time}-A-{ps.type}-{ps.agentId}-{ps.locationId}");
                }
                if (ps.type == (int)PlanStep.types.pickup)
                {
                    sw.WriteLine($"{ps.time}-A-{ps.type}-{ps.agentId}-{ps.locationId}-{ps.orderId}");

                }
                if (ps.type == (int)PlanStep.types.deliver)
                {
                    sw.WriteLine($"{ps.time}-A-{ps.type}-{ps.agentId}-{ps.locationId}-{ps.orderId}");
                }
            }
            sw.Close();
        }
    }
}
