using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
    public class PlanStep
    {
        public int time;
        public int locationId;
        public int type;
        public string agentId;

        public enum types
        {
            movement,
            pickup,
            deliver,
            auction,
        }
    }
}
