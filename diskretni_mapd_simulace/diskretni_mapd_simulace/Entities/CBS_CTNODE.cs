using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
    public class CBS_CTNODE
    {
        public List<CBS_CTNODE> children = new List<CBS_CTNODE>();
        public Dictionary<Agent, List<Location>> agentPaths = new Dictionary<Agent, List<Location>>();
        public Dictionary<Agent, List<CBS_CONSTRAINT>> agentConstraintDict = new Dictionary<Agent, List<CBS_CONSTRAINT>>();
        public int price = -1;
        public List<Agent> conflictAgents = new List<Agent>();
    }
}
