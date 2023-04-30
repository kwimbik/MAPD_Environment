using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Agent :ICloneable
    {
        public string id = "";
        public byte[] color;
        public int solverLocationIndex;
        public Location baseLocation;
        public Location currentLocation;


        public List<Order> orders = new List<Order>();
        public Order assignedOrder = null;

        public Location assignedTask = new Location();

        public Queue<PlanStep> taskList = new Queue<PlanStep>();

        public int movesMade = 0;
        public enum states
        {
            occupied = 0,
            idle = 1,
        }

        public int state;

        public Plan plan = new Plan();

        public object Clone()
        {
            return new Agent
            {
                id = this.id,
                baseLocation = this.baseLocation,
                currentLocation = this.currentLocation,
            };
        }
    }
}
