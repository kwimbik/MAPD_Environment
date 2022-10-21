﻿using System;
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
        public List<Order> orders = new List<Order>();
        public List<Agent> agents = new List<Agent>();
        public int type;

        public enum types
        {
            free,
            wall,
        }

        public static int getDistance(Location loc1, Location loc2)
        {
            return Math.Abs((loc1.coordination[0] - loc2.coordination[0]) + (loc1.coordination[1] - loc2.coordination[1]));
        }
    }
}
