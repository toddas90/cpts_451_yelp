using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    public partial class MainForm : Form
    {
        public event EventHandler<EventArgs> DropDownClosed;
        public class Business
        {
            public string name { get; set; }
            public string state { get; set; }
            public string city { get; set; }
        }
        public MainForm() // Main Form
        {
            Title = "Yelp App"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution

            var layout = new TableLayout(); // Create the TableLayout
            var stateList = new DropDown(); // Create the state drop down menu
            var cityList = new DropDown(); // Create the city drop down menu
            var grid = new GridView { AllowMultipleSelection = false }; // Create the data grid

            createUI(layout, stateList, cityList, grid); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout
            addState(stateList); // Put states in drop down
            addColGrid(grid); // Creates the data grid
        }
        private void addState(DropDown stateList)
        {
            var connection = new NpgsqlConnection(connectionInfo());
            connection.Open();
            var command = new NpgsqlCommand();

            command.Connection = connection;
            command.CommandText = "SELECT distinct state FROM business ORDER BY state";
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                    stateList.Items.Add(reader.GetString(0));
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("SQL Error -  " + ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }
        }
        private void stateList_DropDownClosed(object sender, EventArgs e, DropDown cityList, DropDown stateList)
        {
            var connection = new NpgsqlConnection(connectionInfo());
            connection.Open();
            var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = "SELECT distinct city FROM business WHERE state = " + stateList.SelectedValue.ToString() + " ORDER BY city"; // Broken line
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                    cityList.Items.Add(reader.GetString(0));
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("SQL Error -  " + ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        private string connectionInfo()
        {
            return "Host=localhost; Username=postgres; Database=milestone1db; Password=mustafa";
        }

        private void addColGrid(GridView grid)
        {
            grid.Columns.Add(new GridColumn { HeaderText = "Business Name", Width = 255, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { HeaderText = "State", Width = 60, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { HeaderText = "City", Width = 150, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
        }
        public void createUI(TableLayout layout, DropDown stateList, DropDown cityList, GridView grid)
        {
            grid.Size = new Size(465, 300);
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);
            layout.Rows.Add(new TableRow(
                new Label { Text = "State" },
                TableLayout.AutoSized(stateList)
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "City" },
                TableLayout.AutoSized(cityList)
            ));
            layout.Rows.Add(new TableRow(
                TableLayout.AutoSized(grid)
            ));
            layout.Rows.Add(new TableRow { ScaleHeight = true });
        }
    }
}
