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
        Location location;
        public NewOrderWindow(Location location, Database db)
        {
            this.location = location;
            database = db;
            InitializeComponent();
            createControls();
        }

       public void createControls()
        {
            TextBox tb = new TextBox
            {
                Text = "Order ID",
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 20,
                Width = 150,
            };
            newOrder_grid.Children.Add(tb);

            ComboBox cb = new ComboBox
            {
                Text = "TargetLocation",
                Margin = new Thickness(0, 40, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 20,
                Width = 150,
            };
            foreach (Location loc in database.locations)
            {
                cb.Items.Add($"Location: {loc.id}");
            }
            newOrder_grid.Children.Add(cb);


            Button bt = new Button
            {
                Content = "Accept",
                Margin = new Thickness(0, 80, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 20,
                Width = 150,
            };
            newOrder_grid.Children.Add(bt);

            bt.Click += (sender, e) =>
            {
                Order order = new Order
                {
                    Id = tb.Text,
                    currLocation = location,
                };
                database.orders.Add(order);
                location.orders.Add(order);
                this.Close();
            };
        }
    }
}
