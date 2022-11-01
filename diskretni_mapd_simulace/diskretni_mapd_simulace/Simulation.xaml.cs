using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using diskretni_mapd_simulace.Plan_Tools;
using diskretni_mapd_simulace.Windows;

namespace diskretni_mapd_simulace
{
    /// <summary>
    /// Interaction logic for Simulation.xaml
    /// </summary>
    public partial class Simulation : Window
    {
        Database db = new Database();
        Routing_solver rs;
        Routing_solverManager rsm;
        RoutingSolverResults results;
        Thread assignThread;
        Simulace_Visual sv;
        private int grid_height = 5;
        private int buttonHeight = 90; 

        TextBlock info;



        public Simulation()
        {
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize; 
            InitializeComponent();
            mapParserIO mp = new mapParserIO("map.txt", db);
            sv = new Simulace_Visual(mp.readInputFile(), db);
            db.setTestData(); //TODO: ruzne moznosti tesstovani, pro realny beh smazat
            db.setLocationMap(sv.map.GetLength(0), sv.map[0].Length);
            generateGrid();
            sv.createVisualization();
            sv.Show();
        }

        public void generateGrid()
        {
            for (int i = 0; i < grid_height; i++)
            {
                Simulation_grid.RowDefinitions.Add(new RowDefinition());
            }

            Simulation_grid.Style = (Style)FindResource("GridTheme");
            generateSetupPanel();
            generateMapPanel();
            generatePlanPanel();
        }

        private void generatePlanPanel()
        {
            StackPanel sp = new StackPanel();
            Simulation_grid.Children.Add(sp);
            Grid.SetColumn(sp, 2);
            Grid.SetRowSpan(sp, grid_height);

            Button addAgentBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Add new agent",
                Style  = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            addAgentBtn.Click += (sender, e) =>
            {
                NewAgentWindow naw = new NewAgentWindow(db, this);
                naw.Show();

            };
            sp.Children.Add(addAgentBtn);

            Button addOrderBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Add new order",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };

            addOrderBtn.Click += (sender, e) =>
            {
                NewOrderWindow now = new NewOrderWindow(db, this);
                now.Show();

            };
            sp.Children.Add(addOrderBtn);
        }

        private void generateMapPanel()
        {
            Border sp_bod = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1,0,1,0),
            };
            StackPanel sp = new StackPanel();

            sp_bod.Child = sp;
            Simulation_grid.Children.Add(sp_bod);
            Grid.SetColumn(sp_bod, 1);
            Grid.SetRowSpan(sp_bod, grid_height);

            TextBlock size_tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Text = $@"Size of map: {db.locationMap.GetLength(0)}x{db.locationMap[0].Length}
Agents: {db.agents.Count}
Orders: {db.orders.Count}",
                TextAlignment = TextAlignment.Center,  
            };
            sp.Children.Add(size_tb);
            info = size_tb;

            Button createPlanBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Create plan",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            createPlanBtn.Click += (sender, e) =>
            {
                PlanCreator pc = new PlanCreator(db, db.selectedAlgo);
                pc.Solve();
                PlanReader pr = new PlanReader(sv, db);
                pr.readPlan();
            };
            sp.Children.Add(createPlanBtn);



            Button createPresolveBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Presolve Task assignment",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            createPresolveBtn.Click += (sender, e) =>
            {
                assignOrders();
            };
            sp.Children.Add(createPresolveBtn);

        }

        private void assignOrders()
        {
            Color_assigner ca = new Color_assigner(db);
            //TSP task
            Task TSP = new Task(runTSP);

            //Color assign task
            Task colorAssign = new Task(() => ca.assignColors(results));

            //Visualize Colors tasl
            Task visual = new Task(sv.colorAssignments);

            TSP.Start();
            TSP.Wait();
            colorAssign.Start();
            colorAssign.Wait();

            visual.Start();
        }

        private void generateSetupPanel()
        {
            Border sp_bod = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 0, 1, 0),
            };
            StackPanel sp = new StackPanel();

            sp_bod.Child = sp;
            Simulation_grid.Children.Add(sp_bod);
            Grid.SetColumn(sp_bod, 0);
            Grid.SetRowSpan(sp_bod, grid_height);

            Button fileButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Upload file",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            fileButton.Click += (sender, e) =>
            {
                //add explorer component 
            };
            sp.Children.Add(fileButton);

           

            Button genMap = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Generate map",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            genMap.Click += (sender, e) =>
            {
                //TODO: generate map with specific parameters
            };
            sp.Children.Add(genMap);


            Button chooseAlgo = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Choose algorhitm",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            chooseAlgo.Click += (sender, e) =>
            {
                SelectAlgo sa = new SelectAlgo(db, this);
                sa.Show();
            };
            sp.Children.Add(chooseAlgo);

            Button outputFileBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = "Choose different outputFile",
                Style = (Style)FindResource("MenuButonTheme"),
                Height = buttonHeight,
            };
            outputFileBtn.Click += (sender, e) =>
            {
                //TODO: selects output file in explorer -> forms component
            };
            sp.Children.Add(outputFileBtn);

            TextBlock tb = new TextBlock
            {
                Text = "Current output file: plan.txt",
                TextAlignment = TextAlignment.Center,
                Background = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = buttonHeight,
            };
            sp.Children.Add(tb);


        }

        private void runTSP()
        {
            Routing_solverManager rsm = new Routing_solverManager(db);
            rsm.getSolutionData();
            if (rsm.ordersToProcess.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("All orders have been delivered");
                });
            }
            else
            {
                Routing_solver rs = new Routing_solver(rsm);
                 results = rs.solveProblemAndPrintResults();
            }
        }

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            SimulationController.run = false;
        }

        public void updateUI()
        {
            sv.visualizeAgents();
            sv.visualizeOrders();
            string Text = $@"Size of map: {db.locationMap.GetLength(0)}x{db.locationMap[0].Length}
                Agents: {db.agents.Count}
                Orders: {db.orders.Count}";
            info.Text = Text;
        }
    }
}

