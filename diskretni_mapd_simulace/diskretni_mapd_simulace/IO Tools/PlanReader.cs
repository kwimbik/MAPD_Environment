using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.IO;

namespace diskretni_mapd_simulace
{
    /// <summary>
    /// Read plan from file, executes in on simulace_visual compnent
    /// </summary>
    public class PlanReader
    {
        Thread planThread;

        string file = "plan.txt";

        // Colors definiton

        private byte[] pink = new byte[] { 255, 192, 203 };
        private byte[] red = new byte[] { 255, 0, 0 };
        private byte[] green = new byte[] { 0, 255, 0 };
        private byte[] blue = new byte[] { 0, 0, 255 };

        simulace_visual sv;
        public PlanReader(simulace_visual sv)
        {
            this.sv = sv;
            planThread = new Thread(new ThreadStart(executePlan));
            planThread.Start();
        }

        public void executePlan()
        {
            int time = 0;
            foreach (string line in File.ReadLines(file))
            {
                //Next iteration
                string[] row = line.Split('-');
                if (int.Parse(row[0]) > time) {
                    time++;
                    Thread.Sleep(1000);
                    continue;
                }

                //Agent move
                if (row[1] == "A")
                {
                    sv.changeColor(new int[] { 3, 4 }, pink);
                    sv.changeColor(new int[] { 7, 12 }, green);
                    //TODO: get agent id, get new position, return agent by id, get his color, color new
                    //field with his color, color his previous field with white
                }
            }
        }

    
    }
}
