using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    internal class Database
    {
        public List<Location> locations = new List<Location>();
        public List<Vehicle> vehicles = new List<Vehicle>();
        public List<Order> orders = new List<Order>();
        public int[] gridSize = new int[2]; // najit neco na praci se souradnicema

    }
}
