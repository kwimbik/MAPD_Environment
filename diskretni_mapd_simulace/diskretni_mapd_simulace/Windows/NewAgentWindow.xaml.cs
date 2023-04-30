using Aspose.Pdf.Operators;
using iTextSharp.text;
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
    /// Interaction logic for NewVehicleWindow.xaml
    /// </summary>
    public partial class NewAgentWindow : Window
    {
        Database database;
        Simulation sim;
        Simulace_Visual sv;

        int controlWidth = 50;


        public NewAgentWindow(Database db, Simulation simulationUI, Simulace_Visual sv)
        {
            this.sv = sv;
            sim = simulationUI;
            database = db;
            InitializeComponent();
            createControls();
        }

        public void createControls()
        {
            this.ResizeMode = ResizeMode.NoResize;
            newAgent_grid.Style = (Style)FindResource("GridTheme");
            StackPanel sp = new StackPanel();
            newAgent_grid.Children.Add(sp);

            DockPanel dp1 = new DockPanel();
            sp.Children.Add(dp1);

            Label id_lb = new Label { Content = "Agent Id" };
            dp1.Children.Add(id_lb);

            TextBlock id_tb = new TextBlock
            {
                Text = database.agents.Count.ToString(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextAlignment = TextAlignment.Center,
                Height = 25,
                Width = controlWidth,
            };
            dp1.Children.Add(id_tb);

            DockPanel dp2 = new DockPanel();
            sp.Children.Add(dp2);

            Label location_label = new Label { Content = "Location" };
            dp2.Children.Add(location_label);

            TextBox location_X_tb = new TextBox
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp2.Children.Add(location_X_tb);

            TextBox location_Y_tb = new TextBox
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
                Width = controlWidth,
            };
            dp2.Children.Add(location_Y_tb);
            
            Button random_btn = new Button
            {
                Content = "Random",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = (Style)FindResource("MenuButonTheme"),
                Height = 60,
            };
            sp.Children.Add(random_btn);

            random_btn.Click += (sender, e) =>
            {
                var random = new Random();
               
                int X = random.Next(database.locationMap.Length);
                int Y = random.Next(database.locationMap[0].Length);
                
                location_X_tb.Text = X.ToString();
                location_Y_tb.Text = Y.ToString();
            };

                Button bt = new Button
            {
                Content = "Accept",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = (Style)FindResource("MenuButonTheme"),
                Height = 60,
            };
            sp.Children.Add(bt);

            bt.Click += (sender, e) =>
            {
                int X;
                int Y;
                bool parsedX = int.TryParse(location_X_tb.Text, out X);
                bool parsedY = int.TryParse(location_Y_tb.Text, out Y);

                if (parsedX && parsedY && Y >= 0 && X >= 0)
                {
                    Location location = database.locationMap[X][Y];

                    if (location.type == (int)Location.types.free)
                    {
                        Agent agent = new Agent
                        {
                            id = id_tb.Text,
                            baseLocation = location,
                            currentLocation = location,
                        };
                        database.agents.Add(agent);
                        location.agents.Add(agent);
                        database.scenario.agents.Add(agent);
                        this.Close();
                        sim.updateUI();
                    }
                    else
                    {
                        MessageBox.Show("Invalid location, free location must be selected");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid location, only valid coordinates are accepted");
                }
            };
        }
    }
}
