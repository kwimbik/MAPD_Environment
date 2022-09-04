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
    /// Interaction logic for LocationOptionWindow.xaml
    /// </summary>
    public partial class LocationOptionWindow : Window
    {
        Location location;
        Database database;
        public LocationOptionWindow(Location loc, Database db)
        {
            location = loc;
            database = db;
            InitializeComponent();
            addControls();
            this.Title = $"Location {loc.id}";
            //TODO: Displat current location -> id, curr orders, curr vehicles
            //TODO: buttons to add vehicle, add order
        }

        private void addControls()
        {
            addVehicleInfo();
            addOrderInfo();
        }

        private void addVehicleInfo()
        {
            string tbText = "";
            if (location.vehicles.Count != 0)
            {
                foreach (Vehicle vehicle in location.vehicles)
                {
                    tbText += $"vehicle {vehicle.id} \n";
                }
            }
            else tbText = "No vehicles";

            TextBlock tb = new TextBlock
            {
                Text = tbText,
                Margin = new Thickness(0,0,0,0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 50,
                Width = 150,
                FontSize = 25,
            };
            loc_option_grid.Children.Add(tb);

            Button button = new Button
            {
                Content = "Add new vehicle",
                Margin = new Thickness(0, 100, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 50,
                Width = 150,
            };
            loc_option_grid.Children.Add(button);
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 1);
            Grid.SetColumn(tb,0);
            Grid.SetRow(tb, 0);

            button.Click += (sender, e) =>
            {

            };
        }

        private void addOrderInfo()
        {
            string tbText = "";
            if (location.orders.Count != 0)
            {
                foreach (Order order in location.orders)
                {
                    tbText += $"order {order.Id} \n";
                }
            }
            else tbText = "No orders";

            TextBlock tb = new TextBlock
            {
                Text = tbText,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 50,
                Width = 150,
                FontSize = 25,
            };
            loc_option_grid.Children.Add(tb);

            Button button = new Button
            {
                Content = "Add new order",
                Margin = new Thickness(0, 100, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 50,
                Width = 150,
            };
            loc_option_grid.Children.Add(button);
            Grid.SetColumn(button, 1);
            Grid.SetRow(button, 1);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, 0);

            button.Click += (sender, e) =>
            {
                NewOrderWindow nerOrderWIndow = new NewOrderWindow(location, database);
                nerOrderWIndow.Show();
            };
        }
    }
}
