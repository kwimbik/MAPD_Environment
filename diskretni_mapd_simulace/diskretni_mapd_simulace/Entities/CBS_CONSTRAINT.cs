using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Entities
{
    public class CBS_CONSTRAINT
    {
        public int time;
        public int locationId;
        public int passageId;
        public bool empty = false;
    }
}
