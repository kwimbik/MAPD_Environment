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
        public NewAgentWindow(Database db, Simulation simulationUI)
        {
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

            TextBox tb = new TextBox
            {
                Text = "Agent ID",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 25,
            };
            sp.Children.Add(tb);

            ComboBox loc_cb = new ComboBox
            {
                Text = "Location",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 40,
            };
            foreach (Location loc in database.locations)
            {
                if (loc.type == (int)Location.types.free) loc_cb.Items.Add($"{loc.id}");

            }
            sp.Children.Add(loc_cb);

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
                Location location = database.getLocationByID(int.Parse(loc_cb.Text));

                if (location.type == (int)Location.types.free)
                {
                    Agent vehicle = new Agent
                    {
                        id = tb.Text,
                        baseLocation = location,
                        targetLocation = location,
                    };
                    database.agents.Add(vehicle);
                    location.agents.Add(vehicle);
                    this.Close();
                    sim.updateUI();
                }
                else
                {
                    MessageBox.Show("Invalid location, free location must be selected");
                }                
            };
        }
    }
}
