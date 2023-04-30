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
        string wall = "T@";
        string spcae = ".";
        string file;
        Database db;

        public mapParserIO(string file, Database db)
        {
            this.db = db;
            this.file = file;
        }

        public char[][] readInputFile()
        {
            List<char[]> map = new List<char[]>();

            int id_couter = 0;
            int line_counter = 0;

            int mapWidth = 0;

            try
            {
                foreach (string line in File.ReadLines(file))
                {
                    char[] row = line.ToCharArray();

                    
                    if (line[0] == 'T' || line[0] == '@' || line[0] == '.')
                    {
                        if (mapWidth == 0) mapWidth = row.Length;
                        else if (mapWidth != row.Length)
                        {
                            throw new Exception();
                        }
                        map.Add(row);
                        for (int i = 0; i < row.Length; i++)
                        {
                            createLocation(id_couter++, new int[] { line_counter, i }, row[i]);
                        }
                        line_counter++;
                    }
                }
            }
            catch (Exception)
            {
                return new char[][] { };
            }
            return map.ToArray();
        }

        //Creates location corresponing to one tile
        private void createLocation(int id, int[] coord, char t)
        {
            Location l = new Location();
            l.id = id;
            l.coordination = coord;
            db.locations.Add(l);
            l.type = (t == '.') ? (int)Location.types.free : (int)Location.types.wall;
        }
    }
}
