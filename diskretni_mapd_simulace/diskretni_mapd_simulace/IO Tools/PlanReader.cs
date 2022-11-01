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
        int StopInMs = 250;

        Simulace_Visual sv;
        Database db;
        public PlanReader(Simulace_Visual sv, Database db)
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
            List<Location> agentLocations = new List<Location>();
            List<Location> toBlend = new List<Location>();
            List <string[]> agentLocId = new List<string[]>();

            foreach (string line in File.ReadLines(file))
            {
                //Next iteration
                //TODO: while, nabrat vse v jednom casovem tiku a udelat to pro to, at se mi neprebarvuji agenti zpet
                string[] row = line.Split('-');
                if (int.Parse(row[0]) != time) {

                    //clean white spaces for vehicles
                    foreach (var loc in toBlend)
                    {
                        if (agentLocations.Contains(loc) == false)
                        {
                            sv.changeColor(loc.coordination, new byte[] { 255, 240, 245 });
                        }
                    }
                    foreach (var alID in agentLocId)
                    {
                        Agent a = db.getAgentById(alID[0]);
                        Location l = db.getLocationByID(int.Parse(alID[1]));
                        sv.changeColor(l.coordination, a.color);
                    }

                    time = int.Parse(row[0]);
                    agentLocations.Clear();
                    toBlend.Clear();
                    agentLocId.Clear();
                    Thread.Sleep(StopInMs);
                }

                //Agent move: 'time'-A-'id'-'locationId'
                if (row[1] == "A")
                {
                    Agent a = db.getAgentById(row[2]);
                    Location l = db.getLocationByID(int.Parse(row[3]));

                    toBlend.Add(a.baseLocation);
                    agentLocations.Add(l);
                    agentLocId.Add(new string[] { row[2], row[3] });

                    //move the agent to new location in db
                    a.baseLocation.agents.Remove(a);
                    a.baseLocation = l;
                    l.agents.Add(a);
                }
            }
        }

    
    }
}
