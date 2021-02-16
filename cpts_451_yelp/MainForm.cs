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
            addColGrid(); // Creates the data grid
            this.Content = layout; // Instantiates the layout

            queryState(); // Put states in drop down
            stateList.SelectedValueChanged += new EventHandler<EventArgs>(queryCity);
            cityList.SelectedValueChanged += new EventHandler<EventArgs>(queryBusiness);
        }

        private void executeQuery(string sqlstr, Action<NpgsqlDataReader> myf)
        {
            using (var connection = new NpgsqlConnection(connectionInfo()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = sqlstr;
                    try
                    {
                        Console.WriteLine("Executing Query: " + sqlstr); // For debugging
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                            myf(reader);
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        MessageBox.Show("SQL Error - " + ex.Message.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private string connectionInfo()
        {
            return "Host=localhost; Username=postgres; Database=milestone1db; Password=mustafa";
        }
        public void queryState()
        {
            cityList.Items.Clear();
            data.Clear();

            string cmd = "SELECT distinct state FROM business ORDER BY state";
            executeQuery(cmd, queryStateHelper);
        }
        public void queryCity(object sender, EventArgs e)
        {
            cityList.Items.Clear();
            data.Clear();
            if (stateList.SelectedIndex > -1)
            {
                string cmd = "SELECT distinct city FROM business WHERE state = '" + stateList.SelectedValue.ToString() + "' ORDER BY city";
                executeQuery(cmd, queryCityHelper);
            }
        }
        public void queryBusiness(object sender, EventArgs e)
        {
            data.Clear();
            if (cityList.SelectedIndex > -1)
            {
                string cmd = "SELECT name, state, city FROM business WHERE state = '" + stateList.SelectedValue.ToString() + "' AND city = '" + cityList.SelectedValue.ToString() + "' ORDER BY name";
                executeQuery(cmd, queryBusinessHelper);
                grid.DataStore = data;
            }
        }
        private void queryStateHelper(NpgsqlDataReader R)
        {
            stateList.Items.Add(R.GetString(0));
        }
        private void queryCityHelper(NpgsqlDataReader R)
        {
            cityList.Items.Add(R.GetString(0));
        }
        private void queryBusinessHelper(NpgsqlDataReader R)
        {
            data.Add(new Business() { name = R.GetString(0), state = R.GetString(1), city = R.GetString(2) });
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
