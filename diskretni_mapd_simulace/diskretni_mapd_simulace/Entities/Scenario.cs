using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
   
    public class Scenario
    {
        public string id;
        public string map;
        public List<Agent> agents;
        public List<Order> orders;

        public Scenario(string id, string map, List<Agent> agents, List<Order> orders)
        {
            this.id = id;
            this.map = map;
            this.agents = agents;
            this.orders = orders;
        }

        public Scenario()
        {
            //placeholder for empty scenario
        }
    }
}
