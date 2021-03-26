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
        DropDown zipList = new DropDown();
        DropDown catList = new DropDown();
        GridView grid = new GridView<Business>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

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
            MinimumSize = new Size(800, 600); // Default resolution

            createUI(); // Puts everything where it belongs
            addColGrid(); // Creates the data grid
            this.Content = layout; // Instantiates the layout
            queryState(); // Put states in drop down

            // These attach the event handlers to the specific functions.
            // ie when a value in the stateList is changes, it calls queryCity.
            stateList.SelectedValueChanged += new EventHandler<EventArgs>(queryCity);
            cityList.SelectedValueChanged += new EventHandler<EventArgs>(queryZip);
            zipList.SelectedValueChanged += new EventHandler<EventArgs>(queryCat);
            zipList.SelectedValueChanged += new EventHandler<EventArgs>(queryBusiness);
            catList.SelectedValueChanged += new EventHandler<EventArgs>(queryBusiness);
            grid.SelectionChanged += new EventHandler<EventArgs>(businessWindow);
        }

        // Hard coded credentials for the db, yeet!
        private string connectionInfo()
        {
            return "Host=192.168.0.250; Username=postgres; Database=test_yelp; Password=mustafa";
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
            // Console.WriteLine("Hello from " + B.name); // For debugging!
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
            zipList.Items.Clear();
            catList.Items.Clear();
            data.Clear();

            string cmd = "SELECT distinct businessstate FROM businessaddress ORDER BY businessstate";
            executeQuery(cmd, queryStateHelper);
        }

        // This queries the db for the cities.
        public void queryCity(object sender, EventArgs e)
        {
            // Again, clears the grid data and the cities list.
            cityList.Items.Clear();
            zipList.Items.Clear();
            catList.Items.Clear();
            data.Clear();

            if (stateList.SelectedIndex > -1)
            {
                string cmd = "SELECT distinct businesscity FROM businessaddress WHERE businessstate = '" +
                    stateList.SelectedValue.ToString() + "' ORDER BY businesscity";
                executeQuery(cmd, queryCityHelper);
            }
        }

        public void queryZip(object sender, EventArgs e)
        {
            // Again, clears the grid data and the cities list.
            zipList.Items.Clear();
            catList.Items.Clear();
            data.Clear();

            if (cityList.SelectedIndex > -1)
            {
                string cmd = "SELECT distinct businesspostalcode FROM businessaddress WHERE businessstate = '" +
                    stateList.SelectedValue.ToString() + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' ORDER BY businesspostalcode";
                executeQuery(cmd, queryZipHelper);
            }
        }

        public void queryCat(object sender, EventArgs e)
        {
            // Again, clears the grid data and the cities list.
            catList.Items.Clear();
            data.Clear();

            if (zipList.SelectedIndex > -1)
            {
                string cmd = "SELECT DISTINCT categoryname FROM categories, businessaddress, business WHERE categories.businessid = business.businessid AND business.businessid = businessaddress.businessid AND businessstate = '" +
                    stateList.SelectedValue.ToString() + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "' ORDER BY categoryname";
                executeQuery(cmd, queryCatHelper);
            }
        }

        // This queries the db fto fill in the grid with info.
        public void queryBusiness(object sender, EventArgs e)
        {
            // Again again, clears stuff.
            data.Clear();

            if (catList.SelectedIndex > -1)
            {
                string cmd = @"SELECT DISTINCT businessname, businessstate, businesscity, businesspostalcode, categoryname, business.businessid FROM businessaddress, business, categories
                    WHERE categories.businessid = business.businessid AND business.businessid = businessaddress.businessid AND categories.businessid = businessaddress.businessid AND businessstate = '" + stateList.SelectedValue.ToString()
                    + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "' AND categoryname = '" + catList.SelectedValue.ToString() +
                    "' ORDER BY businessname";
                executeQuery(cmd, queryBusinessHelper);

                // Need to connect the grid to the new data each time I think.
                grid.DataStore = data;
                // grid.UnselectAll(); GRRRRRRRRR
            }
            else if (zipList.SelectedIndex > -1)
            {
                string cmd = @"SELECT DISTINCT businessname, businessstate, businesscity, businesspostalcode, business.businessid FROM businessaddress, business
                    WHERE business.businessid = businessaddress.businessid AND businessstate = '" + stateList.SelectedValue.ToString()
                    + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "' ORDER BY businessname";
                executeQuery(cmd, queryBusinessHelper);

                // Need to connect the grid to the new data each time I think.
                grid.DataStore = data;
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

        private void queryZipHelper(NpgsqlDataReader R)
        {
            zipList.Items.Add(R.GetString(0));
        }
        private void queryCatHelper(NpgsqlDataReader R)
        {
            catList.Items.Add(R.GetString(0));
        }

        // Function that adds the businesses to the grid data store.
        private void queryBusinessHelper(NpgsqlDataReader R)
        {
            if (catList.SelectedIndex > -1)
            {
                data.Add(new Business()
                {
                    name = R.GetString(0),
                    state = R.GetString(1),
                    city = R.GetString(2),
                    zip = R.GetString(3),
                    cat = R.GetString(4),
                    bid = R.GetString(5)
                });
            }
            else
            {
                data.Add(new Business()
                {
                    name = R.GetString(0),
                    state = R.GetString(1),
                    city = R.GetString(2),
                    zip = R.GetString(3),
                    bid = R.GetString(4)
                });
            }
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
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("name"),
                HeaderText = "Business Name",
                Width = 255,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("state"),
                HeaderText = "State",
                Width = 60,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("city"),
                HeaderText = "City",
                Width = 120,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("zip"),
                HeaderText = "Zip Code",
                Width = 80,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("bid"),
                Width = 0,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false,
                Visible = false
            });
        }

        public void createUI()
        {
            grid.Size = new Size(515, 300);
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(20, 20, 20, 20);
            layout.Rows.Add(new TableRow(
                new Label { Text = "State" }
            ));
            layout.Rows.Add(new TableRow(
                TableLayout.AutoSized(stateList)
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "City" }
            ));
            layout.Rows.Add(new TableRow(
                TableLayout.AutoSized(cityList)
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "Zip Code" }
            ));
            layout.Rows.Add(new TableRow(
                TableLayout.AutoSized(zipList)
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "Categories" }
            ));
            layout.Rows.Add(new TableRow(
                TableLayout.AutoSized(catList)
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
            public string zip { get; set; }
            public string cat { get; set; }
            public string bid { get; set; }
        }
    }
}
