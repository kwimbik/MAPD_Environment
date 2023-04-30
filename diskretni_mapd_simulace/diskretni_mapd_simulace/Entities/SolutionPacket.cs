using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace diskretni_mapd_simulace.Entities
{
    public class SolutionPacket
    {
        public int number_of_steps { get; private set; }
        public TimeSpan time { get; private set; }
        public int number_of_orders { get; private set; }

        public int run_time { get; private set; }

        public int nunmber_of_agents { get; private set; }

        public string algorithm = "";
        public string mapName = "";

        public SolutionPacket(int number_of_steps, TimeSpan time, int number_of_orders, int nunmber_of_agents, string algorithm, string mapName, int run_time)
        {
            this.time = time;
            this.number_of_steps = number_of_steps;
            this.number_of_orders = number_of_orders;
            this.algorithm = algorithm;
            this.nunmber_of_agents = nunmber_of_agents; 
            this.mapName = mapName;
            this.run_time = run_time;
        }

        public static SolutionPacket defaultSolutionPacket()
        {
            return new SolutionPacket(0, new TimeSpan(), 0, 0, "", "", 0);
        }
    }
}
