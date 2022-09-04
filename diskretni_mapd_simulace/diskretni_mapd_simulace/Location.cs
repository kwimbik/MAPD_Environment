using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Location
    {
        public int id;
        public int[] coordination = new int[2];

        public static int getDistance(Location loc1, Location loc2)
        {
            return 1; //calculate distance
        }
    }
}
