using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
    public class Passage
    {
        public Location a;
        public Location b;
        public List<int> occupied = new List<int>();
        public int Id;

        //return other end of the passage
        public Location getEnd(Location l)
        {
            if (l.id == a.id) return b;
            if (l.id == b.id) return a;
            else throw (new Exception("no location connected to this passage found"));
        }
    }
}
