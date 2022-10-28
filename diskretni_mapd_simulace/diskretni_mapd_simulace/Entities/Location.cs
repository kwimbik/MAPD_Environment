using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diskretni_mapd_simulace.Entities;

namespace diskretni_mapd_simulace
{
    public class Location
    {
        public int id;
        
        public int[] coordination = new int[2];
        public List<Order> orders = new List<Order>();
        public List<Agent> agents = new List<Agent>();
        public List<Location> accessibleLocations = new List<Location>();
        public List<int> occupiedAt = new List<int>();
        public List<Passage> passages = new List<Passage>();



        //Features for the algorithms, heuristic etc
        public int type;
        public int g;
        public int h;
        public int f;
        public int entranceTime = 0;
        public Location Parent; //to easier backtrack algoriths


        public enum types
        {
            free,
            wall,
        }

        public static int getDistance(Location loc1, Location loc2)
        {
            return Math.Abs((loc1.coordination[0] - loc2.coordination[0]) + (loc1.coordination[1] - loc2.coordination[1]));
        }

        public Passage getPassage(Location l)
        {
            foreach (Passage passage in passages)
            {
                if (passage.a.id == l.id || passage.b.id == l.id) return passage;
            }
            return null;
        }
    }
}
