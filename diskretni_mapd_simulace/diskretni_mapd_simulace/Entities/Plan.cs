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
        public string mapName = "";
        public double serviceTime = 0;
        public List<PlanStep> steps = new List<PlanStep>();
        public SolutionPacket solutionPacket;

        public List<Order> orders = new List<Order>();
        public List<Agent> agents = new List<Agent>();

        public Dictionary<Agent, List<Order>> agentOrderDict = new Dictionary<Agent, List<Order>>();
    }
}
