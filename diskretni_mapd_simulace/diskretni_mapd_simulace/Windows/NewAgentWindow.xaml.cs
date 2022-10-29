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
        Simulace_Visual sv;
        public NewAgentWindow(Database db, Simulace_Visual sv)
        {
            this.sv = sv;
            database = db;
            InitializeComponent();
            createControls();
        }

        public void createControls()
        {
            NewAgent_grid.RowDefinitions.Add(new RowDefinition());
            NewAgent_grid.RowDefinitions.Add(new RowDefinition());
            NewAgent_grid.RowDefinitions.Add(new RowDefinition());

            TextBox tb = new TextBox
            {
                Text = "Agent ID",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            NewAgent_grid.Children.Add(tb);
            Grid.SetRow(tb,0);

            ComboBox loc_cb = new ComboBox
            {
                Text = "Location",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            foreach (Location loc in database.locations)
            {
                if (loc.type == (int)Location.types.free) loc_cb.Items.Add($"{loc.id}");

            }
            NewAgent_grid.Children.Add(loc_cb);
            Grid.SetRow(loc_cb, 1);

            Button bt = new Button
            {
                Content = "Accept",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            NewAgent_grid.Children.Add(bt);
            Grid.SetRow(bt, 2);

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
                    sv.visualizeAgents();
                }
                else
                {
                    MessageBox.Show("Invalid location, free location must be selected");
                }                
            };
        }
    }
}
