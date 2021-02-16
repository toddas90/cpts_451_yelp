using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    public partial class MainForm : Form
    {
        TableLayout layout = new TableLayout();
        DropDown stateList = new DropDown();
        DropDown cityList = new DropDown();
        GridView grid = new GridView<Business> { AllowMultipleSelection = false };
        DataStoreCollection<Business> data = new DataStoreCollection<Business>();

        public event EventHandler<EventArgs> SelectedValueChanged; // Event handler for the dropdown event
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

            createUI(); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout
            addState(); // Put states in drop down
            addColGrid(); // Creates the data grid
            stateList.SelectedValueChanged += new EventHandler<EventArgs>(addCity);
            cityList.SelectedValueChanged += new EventHandler<EventArgs>(addBusiness);
        }

        private string connectionInfo()
        {
            return "Host=localhost; Username=postgres; Database=milestone1db; Password=mustafa";
        }
        public void addState()
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
        public void addCity(object sender, EventArgs e)
        {
            cityList.Items.Clear();
            if (stateList.SelectedIndex > -1)
            {
                var connection = new NpgsqlConnection(connectionInfo());
                connection.Open();
                var command = new NpgsqlCommand();

                command.Connection = connection;
                command.CommandText = "SELECT distinct city FROM business WHERE state = '" + stateList.SelectedValue.ToString() + "' ORDER BY city";
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
        }

        public void addBusiness(object sender, EventArgs e)
        {
            data.Clear();
            if (cityList.SelectedIndex > -1)
            {
                var connection = new NpgsqlConnection(connectionInfo());
                connection.Open();
                var command = new NpgsqlCommand();

                command.Connection = connection;
                command.CommandText = "SELECT name, state, city FROM business WHERE state = '" + stateList.SelectedValue.ToString() + "' AND city = '" + cityList.SelectedValue.ToString() + "' ORDER BY name";
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        data.Add(new Business() { name = reader.GetString(0), state = reader.GetString(1), city = reader.GetString(2) });
                    }
                    grid.DataStore = data;
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
        }
        protected virtual void OnSelectedValueChanged() // I think this should activate when the stateList dropdown is changed?
        {
            EventHandler<EventArgs> handler = SelectedValueChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }
        private void addColGrid() // Adds columns to the graph view
        {
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("name"), HeaderText = "Business Name", Width = 255, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("state"), HeaderText = "State", Width = 60, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("city"), HeaderText = "City", Width = 150, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
        }
        public void createUI() // Parameters for the UI Elements, probably add the columns to this later?
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
