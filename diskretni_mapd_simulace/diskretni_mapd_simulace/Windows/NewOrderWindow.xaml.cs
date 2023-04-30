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

namespace diskretni_mapd_simulace
{
    /// <summary>
    /// Interaction logic for NewOrderWindow.xaml
    /// </summary>
    public partial class NewOrderWindow : Window
    {
        Database database;
        Simulation sim;
        Simulace_Visual sv;
        int controlWidth = 100;
        public NewOrderWindow(Database db, Simulation simulationUI, Simulace_Visual sv)
        {
            this.sv = sv;
            database = db;
            sim = simulationUI;
            InitializeComponent();
            createControls();
        }

       public void createControls()
        {
            newOrder_grid.Style = (Style)FindResource("GridTheme");
            this.ResizeMode = ResizeMode.NoResize;

            StackPanel sp = new StackPanel();
            newOrder_grid.Children.Add(sp);

            DockPanel dp1 = new DockPanel();
            sp.Children.Add(dp1);

            Label lb = new Label { Content = "Order ID", Width = controlWidth };
            dp1.Children.Add(lb);

            TextBlock id_tb = new TextBlock
            {
                Text = database.orders.Count.ToString(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp1.Children.Add(id_tb);


            DockPanel dp2 = new DockPanel();
            sp.Children.Add(dp2);

            Label initLoc_lb = new Label { Content = "Initial Location", Width = controlWidth };
            dp2.Children.Add(initLoc_lb);

            TextBox init_locX_tb = new TextBox
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp2.Children.Add(init_locX_tb);

            TextBox init_locY_tb = new TextBox
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp2.Children.Add(init_locY_tb);

            DockPanel dp3 = new DockPanel();
            sp.Children.Add(dp3);

            Label target_loc_lb = new Label { Content = "Target Location", Width = controlWidth };
            dp3.Children.Add(target_loc_lb);

            TextBox target_locX_tb = new TextBox
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp3.Children.Add(target_locX_tb);

            TextBox target_locY_tb = new TextBox
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp3.Children.Add(target_locY_tb);

            Button random_btn = new Button
            {
                Content = "Random",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = (Style)FindResource("MenuButonTheme"),
                Height = 30,
            };
            sp.Children.Add(random_btn);

            random_btn.Click += (sender, e) =>
            {
                var random = new Random();

                int X1 = random.Next(database.locationMap.Length);
                int Y1 = random.Next(database.locationMap[0].Length);
                int X2 = random.Next(database.locationMap.Length);
                int Y2 = random.Next(database.locationMap[0].Length);

                init_locX_tb.Text = X1.ToString();
                init_locY_tb.Text = Y1.ToString();

                target_locX_tb.Text = X2.ToString();
                target_locY_tb.Text = Y2.ToString();
            };


            Button bt = new Button
            {
                Content = "Accept",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = (Style)FindResource("MenuButonTheme"),
                Height = 30,
            };
            sp.Children.Add(bt);


            bt.Click += (sender, e) =>
            {
                int initX;
                int initY;
                int targetX;
                int targetY;
                bool parsedInitX = int.TryParse(init_locX_tb.Text, out initX);
                bool parsedInitY = int.TryParse(init_locY_tb.Text, out initY);
                bool parsedTargetX = int.TryParse(target_locX_tb.Text, out targetX);
                bool parsedTargetY = int.TryParse(target_locY_tb.Text, out targetY);


                if (parsedInitX && parsedInitY && parsedTargetX && parsedTargetY)
                {
                    Location init_location = database.locationMap[initX][initY];
                    Location targetLocation = database.locationMap[targetX][targetY];


                    if (init_location.type == (int)Location.types.free && targetLocation.type == (int)Location.types.free)
                    {
                        Order order = new Order
                        {
                            id = id_tb.Text,
                            currLocation = init_location,
                            targetLocation = targetLocation,
                        };
                        database.orders.Add(order);
                        init_location.orders.Add(order);
                        this.Close();
                        sim.updateUI();
                    }
                    else
                    {
                        MessageBox.Show("Invalid location, please select correct location");
                    }
                }
                else MessageBox.Show("Enter valid coordination");
            };
        }
    }
}
