using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
    public class ProblemObject
    {
        public List<Order> orders = new List<Order>();
        public List<Agent> agents = new List<Agent>();

        public List<Location> locations = new List<Location>();

        public Location[][] locationMap = new Location[0][];

        public ProblemObject(List<Order> orders, List<Agent> agents, List<Location> locations, Location[][] locationMap)
        {
            this.orders = orders;
            this.agents = agents;
            this.locations = locations;
            this.locationMap = locationMap;
        }
    }
}
