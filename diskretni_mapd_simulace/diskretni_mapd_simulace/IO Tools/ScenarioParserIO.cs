using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace diskretni_mapd_simulace.IO_Tools
{
    public class ScenarioParserIO
    {
        Database db;
        string fileName;
        public ScenarioParserIO(string file, Database db)
        {
            this.db = db;
            this.fileName = file;
        }

        public bool loadScenario()
        {
            List<Agent> agents = new List<Agent>();
            List<Order> orders = new List<Order>();
            int agentCount = 0;
            int orderCount = 0;
            bool loaded_correctly = true;

            try
            {
                foreach (string line in File.ReadLines(fileName))
                {
                    string[] row = line.Split("\t", StringSplitOptions.RemoveEmptyEntries);

                    if (int.TryParse(row[0], out int value))
                    {
                        Order o = parseOrder(row, orderCount);

                        if (o.currLocation.type == (int)Location.types.wall || o.targetLocation.type == (int)Location.types.wall)
                        {
                            //MessageBox.Show($"Invalid scenario, order: {o.id} is placed in the wall, please upload correct scenario");
                            //db.clearScenario();
                            continue;
                        }
                        else
                        {
                            orderCount++;
                            orders.Add(o);
                        }
                    }
                    else if (row[0] == "A")
                    {
                        Agent a = parseAgent(row, agentCount);

                        if (a.baseLocation.type == (int)Location.types.wall)
                        {
                            MessageBox.Show($"Invalid scenario, Agent: {a.id} is placed in the wall, please upload correct scenario");
                            db.clearScenario();
                            return false;
                        }

                        agentCount++;
                        a.baseLocation.agents.Add(a);
                        db.agents.Add(a);
                        agents.Add(a);
                    }
                }

                Scenario sc = new Scenario("0", fileName, agents, orders);
                db.scenario = sc;
            }
            catch (Exception)
            {
                loaded_correctly = false;
            }
            return loaded_correctly;
        }

        //Creates Order with given locations and id
        private Order parseOrder(string[] row, int orderCount)
        {
            //example row: 39,room-64-64-16.map,64,64,46,62,21,62,156.19595947,0
            Location baseLocation = db.locationMap[int.Parse(row[4])][int.Parse(row[5])];
            Location targetLocation = db.locationMap[int.Parse(row[6])][int.Parse(row[7])];
            int timeFrom =  (row.Length > 9) ? timeFrom = int.Parse(row[9]) : 0; //either order has set init time or default 0

            return new Order { id = orderCount.ToString(), currLocation = baseLocation, targetLocation = targetLocation, timeFrom = timeFrom };
        }

        //Creates Agent with given location and id
        private Agent parseAgent(string[] row, int agentCount)
        {
            Location baseLocation = db.locationMap[int.Parse(row[4])][int.Parse(row[5])];
            return new Agent { id = agentCount.ToString(), baseLocation = baseLocation, currentLocation = baseLocation };
        }
    }
}
