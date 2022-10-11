using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    public class Timer
    {
        public static async void Sleep(int seconds)
        {
            Task.Delay(seconds*1000).Wait();
            return;
        }
    }
}
