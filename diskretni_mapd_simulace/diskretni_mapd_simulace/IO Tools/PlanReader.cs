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
        private int cost = 0;

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

                            //if there is order that has not been processed yet, dont overdraw it
                            foreach (var o in loc.orders)
                            {
                                if (o.state == (int)Order.states.pending) sv.changeColor(loc.coordination, o.color);
                            }
                        }
                    }
                    foreach (var alID in agentLocId)
                    {
                        Agent a = db.getAgentById(alID[0]);
                        Location l = db.getLocationByID(int.Parse(alID[1]));
                        sv.changeColor(l.coordination, a.color);
                    }

                    time = int.Parse(row[0]);

                    cost += agentLocations.Count();

                    agentLocations.Clear();
                    toBlend.Clear();
                    agentLocId.Clear();
                    updateSimInfo(time);
                    Thread.Sleep(StopInMs);
                }

                //Agent move: 'time'-A-'id'-'locationId'
                if (row[1] == "A")
                {
                    //movement
                    if (row[2] == "0" )
                    {
                        Agent a = db.getAgentById(row[3]);
                        Location l = db.getLocationByID(int.Parse(row[4]));

                        toBlend.Add(a.baseLocation);
                        agentLocations.Add(l);
                        agentLocId.Add(new string[] { row[3], row[4] });

                        //move the agent to new location in db
                        a.baseLocation.agents.Remove(a);
                        a.baseLocation = l;
                        l.agents.Add(a);
                    }

                    //deliver order
                    if (row[2] == "1")
                    {
                        Agent a = db.getAgentById(row[3]);
                        Location l = db.getLocationByID(int.Parse(row[4]));
                        Order o = db.getOrderById(row[5]);

                        o.state = (int)Order.states.delivered;
                        a.orders.Remove(o);
                        l.orders.Add(o);

                        a.baseLocation.agents.Remove(a);
                        a.baseLocation = l;
                        l.agents.Add(a);
                    }

                    //pickup order
                    if (row[2] == "2")
                    {
                        Agent a = db.getAgentById(row[3]);
                        Location l = db.getLocationByID(int.Parse(row[4]));
                        Order o = db.getOrderById(row[5]);

                        o.state = (int)Order.states.processed;
                        a.orders.Add(o);
                        l.orders.Remove(o);

                        a.baseLocation.agents.Remove(a);
                        a.baseLocation = l;
                        l.agents.Add(a);
                    }

                }
            }
        }

        //TODO: delivered orders # seem sus, fix it
        private void updateSimInfo(int time)
        {
            int delivered = db.getNumOfDeliveredOrders();
            int remaining = db.getNumOfNonDeliveredOrders();
            string text =  @$"Time: {time} 
Cost: {cost} moves
Delivered: {delivered}
Remaining: {remaining}";
            sv.changeInfoText(text);
        }
    }
}
