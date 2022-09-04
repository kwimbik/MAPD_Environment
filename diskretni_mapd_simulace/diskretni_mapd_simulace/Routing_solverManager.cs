using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diskretni_mapd_simulace
{
    internal class Routing_solverManager
    {
        public long[][] TimeMatrix = new long[][] {
            new long []{ 0, 0, 0,  0, 0, 0, 4},
            new long []{ 0, 0, 8,  3, 2, 1, 4},
            new long []{ 0, 8, 0, 11, 1, 2, 4},
            new long []{ 0, 3, 11, 0, 2, 3, 4},
            new long []{ 0, 2, 1, 2, 0,  4, 4},
            new long []{ 0, 1, 2, 3, 4,  0, 4},
            new long []{ 0, 4, 4, 4, 4,  4, 0},
            };
        public int[][] PickupsDeliveries =  new int[][] {
                new int[] { 1, 2 },
                new int[] { 4, 3 },
                new int[] { 5, 6 },
            };
        public long[][] TimeWindows = new long[][] {
            new long[]{ 0, 30},   // depot
            new long[]{ 0, 30 },  // 1
            new long[]{ 0, 30 },// 2
            new long[]{ 0, 30 }, // 3
            new long[]{ 0, 30 }, //4
            new long[]{ 0, 30 }, //4
            new long[]{ 0, 30 } }; //4;

        public long[] Demands = { 0, 8, -8, -8, 8, 5, -5 }; //default value
        public long[] VehicleCapacities = { 12, 12, 12 }; //default value
        public int VehicleNumber = 3; //default value

        //depot values must be different from any pickup or delivery location of an order
        public int Depot = 0; //default value of depot will be all zeros

        public void getSolutionData()
        {
            getTimeMatrix();
            getPickupsAndDeliveries();
            getTimeWindows();
            getDemands();
            getVehicleNumber();
            getDepot();
        }


        public void getTimeMatrix()
        {
            return;
        }


        public void getPickupsAndDeliveries()
        {
            return;
        }

        public void getTimeWindows()
        {
            return;
        }

        public void getDemands()
        {
            return;
        }

        public void getVehicleNumber()
        {
            return;
        }
        public void getDepot()
        {
            return;
        }
    }

}
