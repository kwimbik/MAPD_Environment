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

        simulace_visual sv;
        Database db;
        public PlanReader(simulace_visual sv, Database db)
        {
            this.sv = sv;
            this.db = db;
            
        }

        public void readPlan()
        {
            planThread = new Thread(new ThreadStart(executePlan));
            planThread.Start();
        }


        //IF iteration through databse and its ids is slow, I will make execute expliitely visual with raw coordinates
        private void executePlan()
        {
            int time = 0;
            foreach (string line in File.ReadLines(file))
            {
                //Next iteration
                string[] row = line.Split('-');
                if (int.Parse(row[0]) > time) {
                    time++;
                    Thread.Sleep(1000);
                }

                //Agent move: 'time'-A-'id'-'locationId'
                if (row[1] == "A")
                {
                    Agent a = db.getAgentById(row[2]);
                    Location l = db.getLocationByID(int.Parse(row[3]));
                    sv.changeColor(a.baseLocation.coordination, new byte[] { 255,255,255});
                    sv.changeColor(l.coordination, a.color);

                    //move the agent to new location in db
                    a.baseLocation.agents.Remove(a);
                    a.baseLocation = l;
                    l.agents.Add(a);
                }
            }
        }

    
    }
}
