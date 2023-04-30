using diskretni_mapd_simulace.Entities;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace diskretni_mapd_simulace.Windows
{
    /// <summary>
    /// Interaction logic for StressTestWindow.xaml
    /// </summary>
    public partial class StressTestWindow : Window
    {
        Database db;
        int grid_width = 2;
        int grid_height = 2;
        int sheetRows = 30;
        int buttonHeight = 65;

        Label[] sheet = new Label[30];
        private int numberOfSolutions = 0;

        List<SolutionPacket> solPacketList = new List<SolutionPacket>();
        Simulation sim;

        Label status_lb = new Label();


        PerformanceGraphView orders_steps_pgview = new PerformanceGraphView();
        PerformanceGraphView orders_time_pgview = new PerformanceGraphView();


        PlotView orders_steps_pw = new PlotView();
        PlotView orders_time_pw = new PlotView();

        private string orders_steps = "orders_steps";
        private string orders_time = "orders_time";

        public StressTestWindow(Simulation sim, Database db)
        {
            this.sim = sim;
            this.db = db;
            InitializeComponent();
            generateGrid();
            this.WindowState = WindowState.Maximized;

            //init orders and steps graph
            orders_steps_pw.Model = orders_steps_pgview.MyModel;
            orders_steps_pgview.createGraph(db, orders_steps);

            //init orders and steps graph
            orders_time_pw.Model = orders_time_pgview.MyModel;
            orders_time_pgview.createGraph(db, orders_time);
        }

        public void generateGrid()
        {
            for (int i = 0; i < grid_width; i++)
            {
                stressTestWindow_grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < grid_height; i++)
            {
                //rows for graphs
                stressTestWindow_grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(7, GridUnitType.Star) });
            }

            //rows for buttons
            stressTestWindow_grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });


            stressTestWindow_grid.Style = (Style)FindResource("GridTheme");
            generateResultSheet();
            generateSettgins();
        }

        private void generateResultSheet()
        {
            StackPanel sp = new StackPanel();

            status_lb = new Label
            {
                Content = "In progress",
            };

            sp.Children.Add(status_lb);

            for (int i = 0; i < sheetRows; i++)
            {
                Label l = new Label
                {
                    Content = "Not yet calculated",
                };
                sp.Children.Add(l);
                sheet[i] = l;
            }

            Grid.SetColumn(sp, 0);
            Grid.SetRowSpan(sp, 2);
            stressTestWindow_grid.Children.Add(sp);

            Button stop_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Stop the test",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            

            stop_btn.Click += (sender, e) =>
            {
                sim.timeOut();
            };

            stressTestWindow_grid.Children.Add(stop_btn);
            Grid.SetColumn(stop_btn, 0);
            Grid.SetRow(stop_btn, 2);


            Button exportGraphs_btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Export graphs",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            exportGraphs_btn.Click += (sender, e) =>
            {
               //TODO: export the graphs
            };

            //stressTestWindow_grid.Children.Add(exportGraphs_btn); for now, i dont need it
            Grid.SetColumn(exportGraphs_btn, 1);
            Grid.SetRow(exportGraphs_btn, 2);
        }

        private void generateSettgins()
        {
            //graph for orders and steps
            stressTestWindow_grid.Children.Add(orders_steps_pw);
            Grid.SetColumn(orders_steps_pw, 1);
            Grid.SetRow(orders_steps_pw, 0);

            //graph for orders and time
            stressTestWindow_grid.Children.Add(orders_time_pw);
            Grid.SetColumn(orders_time_pw, 1);
            Grid.SetRow(orders_time_pw, 1);
        }

        public void stressTestFinished()
        {
            status_lb.Content = "Test finished";
        }

        //gets solution packet, update text on stress test window. If max # of entries are reached, overwrite the most recent one and shift by one
        public void updateSolutionSheet(SolutionPacket sp)
        {
            solPacketList.Add(sp);
            int index = numberOfSolutions;
            if (numberOfSolutions >= sheetRows)
            {
                for (int i = 0; i < sheetRows-1; i++)
                {
                    this.Dispatcher.Invoke(new Action(() => sheet[i].Content = sheet[i + 1].Content));
                }
                index = sheetRows-1;
            }
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                sp.time.Hours, sp.time.Minutes, sp.time.Seconds,
                sp.time.Milliseconds / 10);

            this.Dispatcher.Invoke(new Action(() => sheet[index].Content = $"Orders: {sp.number_of_orders}  Steps: {sp.number_of_steps}  Time spent: {elapsedTime}"));
            numberOfSolutions++;
            this.Dispatcher.Invoke(new Action(() =>
            {
                orders_steps_pgview.updateGraph(sp, orders_steps);
                orders_time_pgview.updateGraph(sp, orders_time);
                orders_steps_pw.Model = orders_steps_pgview.MyModel;
                orders_time_pw.Model = orders_time_pgview.MyModel;
            }));
        }
    }
}
