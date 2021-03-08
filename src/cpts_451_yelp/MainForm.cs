using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

// TODO:
// *FIXED* Bug 1: Crashes when you click in the grid but not on an item (empty space).
// *FIXED* Bug 2: When you go into a city and click on a business, it opens the first
//          business in the list, then the one you clicked.
// Implement Feature 1: When you close the main window, close the program.
// Implement Feature 2: Resizing.

namespace cpts_451_yelp
{
    // Main window class.
    public partial class MainForm : Form
    {
        // Lots of variables, kinda gross.
        TableLayout layout = new TableLayout();
        DropDown stateList = new DropDown();
        DropDown cityList = new DropDown();
        GridView grid = new GridView<Business> { AllowMultipleSelection = true,
            AllowEmptySelection = true };

        // Creates a DataStore for the grid. This is how rows work I guess.
        DataStoreCollection<Business> data = new DataStoreCollection<Business>();

        // Event handler for the dropdown selection event.
        public event EventHandler<EventArgs> SelectedValueChanged;

        // Event handler for the grid selection event.
        public event EventHandler<EventArgs> SelectionChanged;


        // Main Form where everything happens
        public MainForm()
        {
            Title = "Yelp App"; // Title of Application
            MinimumSize = new Size(650, 400); // Default resolution

            createUI(); // Puts everything where it belongs
            addColGrid(); // Creates the data grid
            this.Content = layout; // Instantiates the layout
            queryState(); // Put states in drop down

            // These attach the event handlers to the specific functions.
            // ie when a value in the stateList is changes, it calls queryCity.
            stateList.SelectedValueChanged += new EventHandler<EventArgs>(queryCity);
            cityList.SelectedValueChanged += new EventHandler<EventArgs>(queryBusiness);
            grid.SelectionChanged += new EventHandler<EventArgs>(businessWindow);
        }

        // Hard coded credentials for the db, yeet!
        private string connectionInfo()
        {
            return "Host=192.168.0.250; Username=postgres; Database=milestone1db; Password=mustafa";
        }

        // Executes the queries, straight out of the video
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
                        // Console.WriteLine("Executing Query: " + sqlstr); // For debugging
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

        // Creates the Business Details Window and passes along the business id of the
        // business that was clicked on.
        public void businessWindow(object sender, EventArgs e)
        {
            if (grid.SelectedItem == null)
            {
                return;
            }
            Business B = grid.SelectedItem as Business;
            // Console.WriteLine("Hello from " + B.name); For debugging!
            if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
            {
                BusinessForm bwindow = new BusinessForm(B.bid.ToString());
                bwindow.Show();
            }
        }

        // This queries the db for the states.
        public void queryState()
        {
            // Clears the grid data and the cities.
            cityList.Items.Clear();
            data.Clear();

            string cmd = "SELECT distinct state FROM business ORDER BY state";
            executeQuery(cmd, queryStateHelper);
        }

        // This queries the db for the cities.
        public void queryCity(object sender, EventArgs e)
        {
            // Again, clears the grid data and the cities list.
            cityList.Items.Clear();
            data.Clear();

            if (stateList.SelectedIndex > -1)
            {
                string cmd = "SELECT distinct city FROM business WHERE state = '" + stateList.SelectedValue.ToString() + "' ORDER BY city";
                executeQuery(cmd, queryCityHelper);
            }
        }

        // This queries the db fto fill in the grid with info.
        public void queryBusiness(object sender, EventArgs e)
        {
            // Again again, clears stuff.
            data.Clear();

            if (cityList.SelectedIndex > -1)
            {
                string cmd = "SELECT name, state, city, business_id FROM business WHERE state = '" + stateList.SelectedValue.ToString() 
                    + "' AND city = '" + cityList.SelectedValue.ToString() + "' ORDER BY name";
                executeQuery(cmd, queryBusinessHelper);

                // Need to connect the grid to the new data each time I think.
                grid.DataStore = data;
                // grid.UnselectAll(); GRRRRRRRRR
            }
        }

        // Function that adds the states to the list.
        private void queryStateHelper(NpgsqlDataReader R)
        {
            stateList.Items.Add(R.GetString(0));
        }

        // Function that adds the cities to the list.
        private void queryCityHelper(NpgsqlDataReader R)
        {
            cityList.Items.Add(R.GetString(0));
        }

        // Function that adds the businesses to the grid data store.
        private void queryBusinessHelper(NpgsqlDataReader R)
        {
            data.Add(new Business() { name = R.GetString(0), state = R.GetString(1), city = R.GetString(2), bid = R.GetString(3) });
        }

        // Used in the event handling for the DropDown menus. Needs to be here.
        protected virtual void OnSelectedValueChanged()
        {
            EventHandler<EventArgs> handler = SelectedValueChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        //Used in the event handling for the Grid selection. Needs to be here.
        protected virtual void OnSelectionChanged()
        {
            EventHandler<EventArgs> handler = SelectionChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        // Adds the columns to the grid.
        private void addColGrid()
        {
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("name"), HeaderText = "Business Name", 
                    Width = 255, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("state"), HeaderText = "State", 
                    Width = 60, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("city"), HeaderText = "City", 
                    Width = 150, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("bid"), 
                    Width = 0, AutoSize = false, Resizable = false, Sortable = true, Editable = false, Visible = false });
        }

        // This puts all of the UI elements in their places.
        public void createUI()
        {
            grid.Size = new Size(465, 300);
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(20, 20, 20, 20);
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

        // Business class for the data.
        public class Business
        {
            public string name { get; set; }
            public string state { get; set; }
            public string city { get; set; }
            public string bid { get; set; }
        }
    }
}
