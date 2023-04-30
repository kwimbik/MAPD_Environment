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
        public static int mockLocationId = -1;
        
        public int[] coordination = new int[2];
        public List<Order> orders = new List<Order>();
        public List<Agent> agents = new List<Agent>();
        public List<Location> accessibleLocations = new List<Location>();
        public List<int> occupiedAt = new List<int>();
        public List<Passage> passages = new List<Passage>();

        public Agent validationAgent;

        public Dictionary<Location, int> locationDistanceValue = new Dictionary<Location, int>();


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

        public Passage getPassage(Location l)
        {
            foreach (Passage passage in passages)
            {
                if (passage.a.id == l.id || passage.b.id == l.id) return passage;
            }
            return null;
        }

        public static Passage getPassageFromLocation(Location l1, Location l2)
        {
            foreach (var p in l1.passages)
            {
                if (p.getEnd(l1).id == l2.id) return p;
            }
            return null;
        }
    }
}
