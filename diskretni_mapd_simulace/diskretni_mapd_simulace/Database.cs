using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    internal class Database
    {
        List<Location> locations;
        List<Vehicle> vehicles;
        List<Order> orders;
        int[] gridSize = new int[2]; // najit neco na praci se souradnicema
    }
}
