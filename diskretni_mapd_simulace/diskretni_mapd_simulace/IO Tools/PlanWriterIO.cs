using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace diskretni_mapd_simulace.IO_Tools
{
    public static class PlanWriterIO
    {
        public static void writePlan(string fileName, Plan plan)
        {
            File.WriteAllText(fileName, plan.value);
        }
    }
}
