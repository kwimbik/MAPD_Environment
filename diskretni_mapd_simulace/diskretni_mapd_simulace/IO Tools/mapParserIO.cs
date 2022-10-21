using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace diskretni_mapd_simulace
{
    public class mapParserIO
    {
        string file;
        Database db;

        public mapParserIO(string file, Database db)
        {
            this.db = db;
            this.file = file;
        }

        public string[][] readInputFile()
        {
            List<string[]> map = new List<string[]>();

            int id_couter = 0;
            int line_counter = 0;
            foreach (string line in File.ReadLines(file))
            {
                string[] row = line.Split();
                if (line[0] == '0' || line[0] == '1')
                {
                    map.Add(row);
                    for (int i = 0; i < row.Length; i++)
                    {
                        createLocation(id_couter++, new int[] { line_counter, i}, row[i]);
                    }
                }
                //Agent
                else if (line[0] == 'A')
                {
                    Agent a = new Agent
                    {
                        id = row[1],
                        baseLocation = db.getLocationByID(int.Parse(row[2])),
                    };
                    db.agents.Add(a);
                }
                //Order
                else if (line[0] == 'O')
                {
                    Order o = new Order
                    {
                        id = row[1],
                        currLocation = db.getLocationByID(int.Parse(row[2])),
                        targetLocation = db.getLocationByID(int.Parse(row[3])),
                        state = (int)Order.states.pending,
                    };
                    db.orders.Add(o);
                }
                line_counter++;
            }
            return map.ToArray();
        }

        //Creates location corresponing to one tile
        private void createLocation(int id, int[] coord, string t)
        {
            Location l = new Location();
            l.id = id;
            l.coordination = coord;
            db.locations.Add(l);
            l.type = (t == "0") ? (int)Location.types.free : (int)Location.types.wall;
        }
    }
}
