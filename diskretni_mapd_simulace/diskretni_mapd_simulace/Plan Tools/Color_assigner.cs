using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace.Plan_Tools
{
    public class Color_assigner
    {
        Database db;

        public Color_assigner(Database db)
        {
            this.db = db;
        }


        public void assignColors()
        {
            int num_of_agents = db.agents.Count;

        }

        private List<byte[]> obtainColorRange(int num_of_colots)
        {
            return null;
        }
    }
}
