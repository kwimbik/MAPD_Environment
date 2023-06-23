using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Drawing.Design;
using Microsoft.VisualBasic.Logging;
using diskretni_mapd_simulace.Entities;

namespace diskretni_mapd_simulace
{
    /// <summary>
    /// Read plan from file, executes in on simulace_visual compnent
    /// </summary>
    public class PlanReader
    {
        string file = "plan.txt";
        int StopInMs = 15;
        private int cost = 0;
        byte[] targetColor = new byte[] { 255, 215, 0 };
        byte[] blankColor = new byte[] { 255, 240, 245 };
        private bool pause = true;
        private bool planTestMode = false;

        public int readerStatus = (int)PlanReader.status.idle;
        public enum status
        {
            reading,
            idle,
        }

        private bool problemSetup;
        private bool killRead = false;


        byte[] defaultPink = new byte[] { 255, 192, 203 };
        byte[] defaultBlue = new byte[] { 0, 0, 255 };
        byte[] defaultGrey = new byte[] { 105, 105, 105 };


        private int maxAgentsToColor = 6;
        byte[][] colors = new byte[6][] {
                 new byte[]{ 255, 0, 0 },
                 new byte[] { 255, 165, 0 },
                 new byte[]{ 0, 0, 255 },
                 new byte[]{ 255, 0, 165 },
                 new byte[]{ 0, 255, 165 },
                 new byte[] { 165, 0, 255 }};

        string[] orderColors = new string[6] { "red", "orange", "blue", "pink", "green", "purple" };
        string defaultGreyBox = "grey";
        string targetColorBox = "yellow";


        public Simulace_Visual sv { get; set; }
        Database db;
        List<Agent> agentsToPlan = new List<Agent>();
        List<Order> ordersToPlan = new List<Order>();
        List<Order> initialOrders = new List<Order>();

        List<Location> locationForMapCreation = new List<Location>();

        public PlanReader(Database db)
        {
            this.db = db;
        }

        public void speedUp()
        {
            if (StopInMs > 10) StopInMs /= 2;
        }

        public void slowDown()
        {
            if (StopInMs < 100) StopInMs *= 2;
        }

        private void resetMap()
        {
            foreach (var l in locationForMapCreation)
            {
                sv.changeColor(l.coordination, blankColor);
            }
        }

        public void pausePlan() => pause = true;
        public void resumePlan() => pause = false;
       
        //we read this plan upon loading new plan
        public void readPlan(string fileName)
        {
            try
            {
                this.file = fileName;
                clearReader();
                Task planTask = new Task(executePlan);
                planTask.Start();
            }
            catch
            {
                MessageBox.Show("Previous plan not finished yet");
            }
        }

        private void clearReader()
        {
            ordersToPlan.Clear();
            initialOrders.Clear();
            agentsToPlan.Clear();
            locationForMapCreation.Clear();
        }

        public void stopPlanExecution()
        {
            killRead = true;
        }

        public void addAgentsToPlan(Agent a)
        {
            //plan for this agent
            if (agentsToPlan.Contains(a) == false) agentsToPlan.Add(a);

            sv.changeColor(a.currentLocation.coordination, a.color);

            foreach (var o in a.orders)
            {
                //if (o.state == (int)Order.states.pending) sv.changeColor(o.currLocation.coordination, o.color);
                if (o.state == (int)Order.states.pending) sv.displayOrder(o.currLocation.coordination, o.colorBox);
                //if (o.state == (int)Order.states.processed) sv.changeColor(o.targetLocation.coordination, targetColor);
                if (o.state == (int)Order.states.processed) sv.displayOrder(o.targetLocation.coordination, targetColorBox);
            }
        }

        public void removeAgentsToPlan(Agent a)
        {
            agentsToPlan.Remove(a);
            sv.changeColor(a.currentLocation.coordination, blankColor);

            foreach (var o in a.orders)
            {
                if (o.state == (int)Order.states.pending) sv.changeColor(o.currLocation.coordination, blankColor);
                if (o.state == (int)Order.states.processed) sv.changeColor(o.targetLocation.coordination, blankColor);
            }
        }

        public void HighlightAgent(Agent a)
        {
            //TODO implement highlight function
        }

        public void removeHighlightFromAgent(Agent a)
        {

        }

        public void setColorsDefault()
        {
            foreach (var agent in agentsToPlan)
            {
                agent.color = defaultPink;
                sv.changeColor(agent.currentLocation.coordination, agent.color);
            }

            foreach (var order in initialOrders)
            {
                order.color = defaultBlue;
                order.colorBox = "blue";
                if (order.state != (int)Order.states.delivered)
                {
                    //sv.changeColor(order.currLocation.coordination, order.color);
                    //sv.displayOrder(order.currLocation.coordination, order.colorBox);
                }
            }
        }

        public void setColorsPallete()
        {
            
            if (agentsToPlan.Count <= maxAgentsToColor)
            {
                foreach (var agent in agentsToPlan)
                {
                    agent.color = colors[int.Parse(agent.id)];
                    sv.changeColor(agent.currentLocation.coordination, agent.color);
                    foreach (Order order in agent.orders)
                    {
                        order.colorBox = orderColors[int.Parse(agent.id)];
                        if (order.state != (int)Order.states.delivered)
                        {
                            sv.displayOrder(order.currLocation.coordination, order.colorBox);
                            //sv.changeColor(order.currLocation.coordination, order.color);
                        }
                    }
                }
            }
            else
            {
                setColorsDefault();
            }
        }

        private void setFrequencies(double freq, List<Order> orders)
        {
            double counter = 0;
            int time = 1;
            for (int i = 0; i < orders.Count; i++)
            {
                if (freq < 1)
                {
                    while (counter < 1)
                    {
                        time++;
                        counter += freq;
                    }
                    orders[i].timeFrom = time;
                    counter = 0;
                }
                else
                {
                    if (counter < freq)
                    {
                        counter++;
                    }
                    else
                    {
                        time++;
                        counter = 1;
                    }
                    orders[i].timeFrom = time;
                }
            }
        }


        //IF iteration through databse and its ids is slow, I will make execute expliitely visual with raw coordinates
        private void executePlan()
        {
            //reset for the new plan
            cost = 0;
            readerStatus = (int)PlanReader.status.reading;
            problemSetup = true;
            int time = 0;
            List<Location> agentLocations = new List<Location>();
            List<Location> toBlend = new List<Location>();
            List <string[]> agentLocId = new List<string[]>();

            //initialization of a plan
            foreach (var l in db.locations)
            {
                l.orders.Clear();
                l.agents.Clear();
            }
            sv.resetMap();



            foreach (string line in File.ReadLines(file))
            {
                //Next iteration
                string[] row = line.Split('-');

                //setup problem -> load agents and orders
                if (problemSetup)
                {
                    //Load agents participating in plan
                    if (row[0] == "A")
                    {
                        Location l = db.getLocationByID(int.Parse(row[2]));

                        //reads incorrectly placed order or agent
                        if (l.type == (int)Location.types.wall)
                        {
                            MessageBox.Show($"Location: {l.coordination[0] + "," + l.coordination[1]}, id: {l.id} cannot contain agent or oder");
                            sv.resetMap();
                            return;
                        }
                        Agent agent = new Agent { id = row[1], baseLocation = l, currentLocation = l,  color = defaultPink };

                        l.agents.Add(agent);
                        agentsToPlan.Add(agent);
                        continue;
                    }

                    //Load orders
                    else if (row[0] == "O")
                    {
                        Location l = db.getLocationByID(int.Parse(row[2]));

                        //reads incorrectly placed order or agent
                        if (l.type == (int)Location.types.wall)
                        {
                            MessageBox.Show($"Location: {l.coordination[0] + "," + l.coordination[1]}, id: {l.id} cannot contain agent or oder");
                            sv.resetMap();
                            return;
                        }

                        Order order = new Order { id = row[1], currLocation = l, targetLocation = db.getLocationByID(int.Parse(row[3])), color = defaultBlue };

                        //ordersToPlan.Add(order);
                        initialOrders.Add(order);
                        l.orders.Add(order);
                        continue;
                    }
                    else if (row[0] == "AS")
                    {
                        //Assignes orders to the agents
                        Agent agent = getAgentById(row[1]);
                        string[] orderIds = row[2].Split(',');

                        List<Order> orders = new List<Order>();
                        foreach (var orderID in orderIds)
                        {
                            orders.Add(getOrderById(orderID));
                        }
                        agent.orders = orders;
                        continue;
                    }
                    else
                    {
                        setColorsPallete();
                        updateSimInfo(time);
                        problemSetup = false;
                        setFrequencies(0.5, initialOrders);
                        foreach (var a in agentsToPlan)
                        {
                            if (a.orders.Count > 0)
                            {
                                a.orders[0].state = (int)Order.states.targeted;
                            }
                        }
                        continue;
                    }

                }
                //check for unpsause every 100ms
                while (pause)
                {
                    Thread.Sleep(100);
                    if (killRead)
                    {
                        sv.resetMap();
                        killRead = false;
                        return;
                    }
                }

                //closes the file and stops the reading
                if (killRead)
                {
                    sv.resetMap();
                    killRead= false;
                    return;
                }

                
                else if (int.Parse(row[0]) != time) {

                    foreach (var o in initialOrders.ToList())
                    {
                        if (o.timeFrom <= int.Parse(row[0]))
                        {
                            initialOrders.Remove(o);
                            ordersToPlan.Add(o);
                        }
                    }
                    
                    //draw all oders first, if agents overstep order -> agent color has priority
                    foreach (var o in ordersToPlan)
                    {
                        //if (o.state == (int)Order.states.pending) sv.changeColor(o.currLocation.coordination, defaultGrey);
                        if (o.state == (int)Order.states.pending) sv.displayOrder(o.currLocation.coordination, defaultGreyBox);
                        //if (o.state == (int)Order.states.targeted) sv.changeColor(o.currLocation.coordination, o.color);
                        if (o.state == (int)Order.states.targeted) sv.displayOrder(o.currLocation.coordination, o.colorBox);
                    }

                    foreach (var alID in agentLocId)
                    {
                        Agent a = getAgentById(alID[0]);
                        Location l = db.getLocationByID(int.Parse(alID[1]));
                        if (agentsToPlan.Contains(a)) sv.changeColor(l.coordination, a.color);

                        //move the agent to new location in db
                        a.currentLocation.agents.Remove(a);
                        a.currentLocation = l;
                        l.agents.Add(a);
                    }

                    //clean white spaces for vehicles
                    foreach (var loc in toBlend)
                    {
                        if (agentLocations.Contains(loc) == false)
                        {
                            sv.changeColor(loc.coordination, blankColor);
                        }
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
                    Agent a = getAgentById(row[3]);
                    Location l = db.getLocationByID(int.Parse(row[4]));

                    //movement
                    if (row[2] == "0" )
                    {
                        toBlend.Add(a.currentLocation);
                        agentLocations.Add(l);
                        agentLocId.Add(new string[] { row[3], row[4] });
                    }

                    //deliver order
                    if (row[2] == "2")
                    {
                        Order o = getOrderById(row[5]);
                        int orderIndex = a.orders.IndexOf(o) + 1;
                        if (a.orders.Count >= orderIndex + 1)
                        {
                            a.orders[orderIndex].state = (int)Order.states.targeted;
                        }

                        o.state = (int)Order.states.delivered;
                        l.orders.Add(o);

                        a.currentLocation.agents.Remove(a);
                        a.currentLocation = l;
                        l.agents.Add(a);
                    }

                    //pickup order
                    if (row[2] == "1")
                    {
                        Order o = getOrderById(row[5]);
                        o.state = (int)Order.states.processed;

                        //color the target location
                        //if (agentsToPlan.Contains(a))  sv.changeColor(o.targetLocation.coordination, targetColor);
                        if (agentsToPlan.Contains(a)) sv.displayOrder(o.targetLocation.coordination, targetColorBox);

                        o.state = (int)Order.states.processed;
                        l.orders.Remove(o);

                        a.currentLocation.agents.Remove(a);
                        a.currentLocation = l;
                        l.agents.Add(a);
                    }
                }
                //final sim info
                updateSimInfo(time);
            }
            pausePlan();
            sv.forcePause();
            readerStatus = (int)PlanReader.status.idle;
        }

        public Agent getAgentById(string id)
        {
            foreach (Agent v in agentsToPlan)
            {
                if (v.id == id) return v;
            }
            return null;
        }

        public Order getOrderById(string Id)
        {
            foreach (Order o in initialOrders)
            {
                if (o.id == Id) return o;
            }
            foreach (Order o in ordersToPlan)
            {
                if (o.id == Id) return o;
            }
            return null;
        }

        private int getNumOfDeliveredOrders()
        {
            int count = 0;

            foreach (var o in ordersToPlan)
            {
               if (o.state == (int)Order.states.delivered) count++;
            }
            return count;
        }

        private int getNumOfNonDeliveredOrders(int delivered)
        {
            return ordersToPlan.Count - delivered;
        }


            private void updateSimInfo(int time)
        {
            int delivered = getNumOfDeliveredOrders();
            int remaining = getNumOfNonDeliveredOrders(delivered);
            string text =  @$"Time: {time} 
Cost: {cost} moves
Delivered: {delivered}
Remaining: {remaining}";
            sv.changeInfoText(text);
        }
    }
}
