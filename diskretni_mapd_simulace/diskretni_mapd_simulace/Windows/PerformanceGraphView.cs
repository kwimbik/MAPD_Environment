using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diskretni_mapd_simulace.Entities;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Microsoft.VisualBasic;
using System.Windows;

namespace diskretni_mapd_simulace.Windows
{
    public class PerformanceGraphView
    {
        string mapName = "";
        string algorithmUsed = "";
        string outputFileName = "";
        int numberOfAgents = 0;
        const string fileExtenstion =  ".stpg" ;  //stress test performance graph

        Database db;

        LineSeries linSeries;


        List<SolutionPacket> solutionPackets = new List<SolutionPacket>();

        public PlotModel MyModel { get; private set; }

        public PerformanceGraphView() 
        {
            this.MyModel = new PlotModel { Title = "Performance graph" };
        }

        public void updateGraph(SolutionPacket solutionPacket, string value)
        {
            if (value == "orders_steps")
            {
                linSeries.Points.Add(new DataPoint(solutionPacket.number_of_orders, solutionPacket.number_of_steps));
            }
            else if (value == "orders_time")
            {
                double time = 0;
                time += (double)solutionPacket.time.Milliseconds / 1000;
                time += solutionPacket.time.Seconds;
                time += solutionPacket.time.Minutes * 60;
                time += solutionPacket.time.Hours * 3600;
                linSeries.Points.Add(new DataPoint(solutionPacket.number_of_orders, time));
            }
            else
            {
                MessageBox.Show("Incorrect graph input");
                return;
            }

            MyModel.InvalidatePlot(true);
        }

        public void createGraph(Database db, string graphType)
        {
            mapName = db.mapName;
            algorithmUsed = db.selectedAlgo;

            outputFileName = mapName + fileExtenstion;
            numberOfAgents = db.agents.Count();

            MyModel.Title = $"Map: {mapName}, Algorithm: {algorithmUsed}, Agents: {numberOfAgents}";

            LinearAxis orderAxisB = new LinearAxis
            {
                Position = AxisPosition.Bottom,
            };

            LinearAxis stepsAxisL = new LinearAxis
            {
                Position = AxisPosition.Left,
            };

            if (graphType == "orders_steps")
            {
                stepsAxisL.Title = "Steps";
                orderAxisB.Title = "Orders";
            }
            else if (graphType == "orders_time")
            {
                stepsAxisL.Title = "Seconds";
                orderAxisB.Title = "Orders";
            }

            LineSeries ls = new LineSeries() { Title = "points" };
            MyModel.Axes.Add(orderAxisB);
            MyModel.Axes.Add(stepsAxisL);

            linSeries = ls;
            MyModel.Series.Add(ls);
        }
    }
}
