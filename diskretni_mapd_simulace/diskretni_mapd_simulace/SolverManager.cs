using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    internal class SolverManager
    {
        public long[][] TimeMatrix;
        public int[][] PickupsDeliveries;
        public long[][] TimeWindows;
        public long[] Demands = { 0, 8, -8, -8, 8, 5, -5 }; //default value
        public long[] VehicleCapacities = { 12, 12, 12 }; //default value
        public int VehicleNumber = 1; //default value

        //depot values must be different from any pickup or delivery location of an order
        public int Depot = 1; //default value of depot will be all zeros


        public void getTimeMatrix()
        {

        }

        public void getPickupsAndDeliveries()
        {

        }

        public void getTimeWindows()
        {

        }

        public void getDemands()
        {

        }

        public void getVehicleNumber()
        {

        }
        public void getDepot()
        {

        }
    }

}
