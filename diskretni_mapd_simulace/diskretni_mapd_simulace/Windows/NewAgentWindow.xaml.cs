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

        public NewAgentWindow(Database db)
        {
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

            TextBox loc_txb = new TextBox
            {
                Text = "Location",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            NewAgent_grid.Children.Add(loc_txb);
            Grid.SetRow(loc_txb, 1);

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
                Location location = database.getLocationByID(int.Parse(loc_txb.Text));

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
                }
                else
                {
                    MessageBox.Show("Invalid location, free location must be selected");
                }                
            };
        }
    }
}
