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
        public NewOrderWindow(Database db, Simulation simulationUI)
        {
            database = db;
            sim = simulationUI;
            InitializeComponent();
            createControls();
        }

       public void createControls()
        {
            newOrder_grid.RowDefinitions.Add(new RowDefinition());
            newOrder_grid.RowDefinitions.Add(new RowDefinition());
            newOrder_grid.RowDefinitions.Add(new RowDefinition());
            newOrder_grid.RowDefinitions.Add(new RowDefinition());
            TextBox id_tb = new TextBox
            {
                Text = "Order ID",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            newOrder_grid.Children.Add(id_tb);
            Grid.SetRow(id_tb, 0);


            ComboBox initPostition_cb = new ComboBox
            {
                Text = "Initial location",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            foreach (Location loc in database.locations)
            {
                if (loc.type == (int)Location.types.free)  initPostition_cb.Items.Add($"{loc.id}");
            }
            newOrder_grid.Children.Add(initPostition_cb);
            Grid.SetRow(initPostition_cb, 1);



            ComboBox tarhetPos_cb = new ComboBox
            {
                Text = "TargetLocation",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            foreach (Location loc in database.locations)
            {
                if (loc.type == (int)Location.types.free) tarhetPos_cb.Items.Add($"{loc.id}");

            }
            newOrder_grid.Children.Add(tarhetPos_cb);
            Grid.SetRow(tarhetPos_cb, 2);


            Button bt = new Button
            {
                Content = "Accept",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            newOrder_grid.Children.Add(bt);
            Grid.SetRow(bt, 3);


            //TODO: check na validitu lokace
            bt.Click += (sender, e) =>
            {
                Location init_location = database.getLocationByID(int.Parse(initPostition_cb.Text));
                Location targetLocation = database.getLocationByID(int.Parse(tarhetPos_cb.Text));

                if (init_location.type == (int) Location.types.free && targetLocation.type == (int)Location.types.free)
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
                    MessageBox.Show("Invalid location, non wall location must be selected");
                }
            };
        }
    }
}
