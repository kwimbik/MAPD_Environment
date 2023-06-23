using diskretni_mapd_simulace.Entities;
using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace diskretni_mapd_simulace.BT_Tools
{
    public static class ExperimentDataWriter
    {
        private static string CBSINFOFILE = "BT_data/CBSINFOFILE_exp3.txt";
        private static string getCSVFronSolutionPacket(SolutionPacket sp, double frequency, int seed, int movements, int waiting, double cost)
        {
            //FORMAT is: MapName, ScenarioName, Algorithm, Agents,Orders,frequency, MakeSpan,ServiceTime, RunTime, Movements, Waitings, Cost, Seed,
            return $"{sp.mapName},{sp.scenarioName},{sp.algorithm},{sp.nunmber_of_agents},{sp.number_of_orders},{frequency},{sp.makespan},{sp.serviceTime},{sp.time},{movements},{waiting},{cost},{seed}";
        }

        public static void WritedData(SolutionPacket sp, double frequency, int seed, string filename, int movements, int waiting, double cost)
        {
            using (StreamWriter w = File.AppendText(filename))
            {
                w.WriteLine(getCSVFronSolutionPacket(sp, frequency, seed, movements, waiting, cost));
            }
        }

        public static void writeCBSDATA(List<int> CSTdepths, int numOfAgents)
        {
            using (StreamWriter w = File.AppendText(CBSINFOFILE))
            {
                w.WriteLine(string.Join(",", numOfAgents));
                w.WriteLine(string.Join(",", CSTdepths));
            }
        }
    }
}
