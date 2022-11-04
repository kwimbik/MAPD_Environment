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

namespace diskretni_mapd_simulace.Windows
{
    /// <summary>
    /// Interaction logic for SImulationSettingsWindow.xaml
    /// </summary>
    public partial class SImulationSettingsWindow : Window
    {
        Database db;
        PlanReader pr;


        public SImulationSettingsWindow(Database db, PlanReader pr)
        {
            this.db = db;
            this.pr = pr;
            InitializeComponent();
            createWindow();
        }

        private void createWindow()
        {
            Grid grid = new Grid
            {
                Style = (Style)FindResource("GridTheme"),
            };
            StackPanel sp = new StackPanel();
            grid.Children.Add(sp);

            foreach (var a in db.agents)
            {
                WrapPanel wp = new WrapPanel();
                Label lb = new Label { Content = $"Agent: {a.id}", Margin = new Thickness(0,0,0,10) };
                wp.Children.Add(lb);

                RadioButton rb1 = new RadioButton
                {
                    Content = "Visible",
                    IsChecked = true,
                };
                wp.Children.Add(rb1);

                RadioButton rb2 = new RadioButton
                {
                    Content = "Highlight",
                    IsChecked=false,
                };
                wp.Children.Add(rb2);

                
                rb1.Checked += (sender, e) =>
                {
                    pr.addAgentsToPlan(a);
                };

                rb1.Unchecked += (sender, e) =>
                {
                    pr.removeAgentsToPlan(a);
                };

                rb2.Checked += (sender, e) =>
                {
                    pr.HighlightAgent(a);
                };

                rb2.Unchecked += (sender, e) =>
                {
                    pr.removeHighlightFromAgent(a);
                };

                sp.Children.Add(wp);
            }
            Button acp_btn = new Button
            {
                Style = (Style)FindResource("MenuButonTheme"),
                Content = "Close",
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            acp_btn.Click += (sender, e) =>
            {
                this.Close();
            };
            sp.Children.Add(acp_btn);
            this.Content = grid;
        }
    }
}
