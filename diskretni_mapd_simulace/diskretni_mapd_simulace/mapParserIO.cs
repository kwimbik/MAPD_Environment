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
        
        public mapParserIO(string file)
        {
            this.file = file;
        }

        public string[][] readMap()
        {
            List<string[]> map = new List<string[]>();

            foreach (string line in File.ReadLines(file))
            {
                string[] row = line.Split();
                map.Add(row);
            }
            return map.ToArray();
        }
    }
}
