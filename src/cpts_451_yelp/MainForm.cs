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
        DynamicLayout layout = new DynamicLayout();
        GroupBox selectionBox = new GroupBox(); // Experimental boxes around groups of items.
        DropDown stateList = new DropDown();
        ListBox cityList = new ListBox
        {
            Size = new Size(150, 100)
        };
        ListBox zipList = new ListBox
        {
            Size = new Size(150, 100)
        };
        ListBox catList = new ListBox
        {
            Size = new Size(150, 100)
        };
        ListBox selectedCats = new ListBox
        {
            Size = new Size(150, 100)
        };
        Button add = new Button
        {
            Text = "Add"
        };
        Button search = new Button
        {
            Text = "Search"
        };
        Button remove = new Button
        {
            Text = "Remove"
        };
        GridView grid = new GridView<Business>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        // Creates a DataStore for the grid. This is how rows work I guess.
        DataStoreCollection<Business> data = new DataStoreCollection<Business>();

        public event EventHandler<EventArgs> SelectedValueChanged;

        // Event handler for the grid selection event.
        public event EventHandler<EventArgs> SelectionChanged;

        public event EventHandler<EventArgs> Click;


        // Main Form where everything happens
        public MainForm()
        {
            Title = "Yelp App"; // Title of Application
            MinimumSize = new Size(1280, 720); // Default resolution

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
            add.Click += new EventHandler<EventArgs>(addSelected);
            remove.Click += new EventHandler<EventArgs>(removeSelected);
            search.Click += new EventHandler<EventArgs>(queryBusiness);
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
            try
            {
                if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
                {
                    BusinessForm bwindow = new BusinessForm(B.bid.ToString());
                    bwindow.Show();
                }
            }
            catch (System.InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
        }

        // This queries the db for the states.
        public void queryState()
        {
            // Clears the grid data and the cities.
            cityList.Items.Clear();
            zipList.Items.Clear();
            catList.Items.Clear();
            selectedCats.Items.Clear();
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
            selectedCats.Items.Clear();
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
            selectedCats.Items.Clear();
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
            selectedCats.Items.Clear();
            data.Clear();

            if (zipList.SelectedIndex > -1)
            {
                string cmd = "SELECT DISTINCT categoryname FROM categories, businessaddress, business WHERE categories.businessid = business.businessid AND business.businessid = businessaddress.businessid AND businessstate = '" +
                    stateList.SelectedValue.ToString() + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "' ORDER BY categoryname";
                executeQuery(cmd, queryCatHelper);
            }
        }

        // This queries the db fto fill in the grid with info.
        // public void queryBusiness(object sender, EventArgs e)
        // {
        //     // Again again, clears stuff.
        //     data.Clear();

        //     if (zipList.SelectedIndex > -1)
        //     {
        //         string cmd = @"SELECT DISTINCT businessname, businessstate, businesscity, businesspostalcode, business.businessid FROM businessaddress, business
        //             WHERE business.businessid = businessaddress.businessid AND businessstate = '" + stateList.SelectedValue.ToString()
        //             + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "' ORDER BY businessname";
        //         executeQuery(cmd, queryBusinessHelper);
        //     }

        //     // Need to connect the grid to the new data each time I think.
        //     grid.DataStore = data;
        // }

        public void queryBusiness(object sender, EventArgs e)
        {
            data.Clear();
            if (selectedCats.Items.Count > 0)
            {
                string cmd = @"SELECT DISTINCT businessname, businessstate, businesscity, businesspostalcode, business.businessid, categoryname FROM businessaddress, business, 
                (SELECT DISTINCT businessid, categoryname FROM categories WHERE " + stringifyCategories(selectedCats.Items) + ") as narrowedCats " +
                    "WHERE narrowedCats.businessid = business.businessid AND business.businessid = businessaddress.businessid AND narrowedCats.businessid = businessaddress.businessid AND businessstate = '"
                    + stateList.SelectedValue.ToString() + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "'" +
                    "ORDER BY businessname";
                executeQuery(cmd, queryBusinessHelper);
                grid.DataStore = data;
            }
            else if (zipList.SelectedIndex > -1)
            {
                string cmd = @"SELECT DISTINCT businessname, businessstate, businesscity, businesspostalcode, business.businessid FROM businessaddress, business
                    WHERE business.businessid = businessaddress.businessid AND businessstate = '" + stateList.SelectedValue.ToString()
                    + "' AND businesscity = '" + cityList.SelectedValue.ToString() + "' AND businesspostalcode = '" + zipList.SelectedValue.ToString() + "' ORDER BY businessname";
                executeQuery(cmd, queryBusinessHelper);
                grid.DataStore = data;
            }
        }

        public string stringifyCategories(ListItemCollection lst)
        {
            string ret = " categoryname =" + "'" + lst[0].ToString() + "'";
            for (int i = 1; i < lst.Count; i++)
            {
                ret += " AND categoryname = " + "'" + lst[i].ToString() + "'";
            }
            return ret;
        }

        public void addSelected(object sender, EventArgs e)
        {
            for (int i = 0; i < selectedCats.Items.Count; i++)
            {
                if (selectedCats.Items[i].ToString() == catList.SelectedValue.ToString())
                {
                    Console.WriteLine("Item found");
                    return;
                }
            }

            try
            {
                selectedCats.Items.Add(catList.SelectedValue.ToString());
            }
            catch (System.NullReferenceException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("Please select a category first!");
            }
        }

        public void removeSelected(object sender, EventArgs e)
        {
            try
            {
                selectedCats.Items.RemoveAt(selectedCats.SelectedIndex);
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("Please select a category first!");
            }
            finally
            {

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
            data.Add(new Business()
            {
                name = R.GetString(0),
                state = R.GetString(1),
                city = R.GetString(2),
                zip = R.GetString(3),
                bid = R.GetString(4)
            });
        }

        // Used in the event handling for the DropDown menus. Needs to be here.

        protected virtual void OnSelectedValueChangec()
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

        protected virtual void OnClick()
        {
            EventHandler<EventArgs> handler = Click;
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
            layout.Padding = new Padding(10, 0, 10, 10);
            grid.Size = new Size(515, 400);
            layout.DefaultSpacing = new Size(5, 5);

            layout.BeginHorizontal();

            layout.BeginVertical();
            layout.BeginGroup("Location", new Padding(10, 10, 10, 10));

            layout.BeginHorizontal();
            //layout.BeginVertical(padding: new Padding(0, 10, 0, 10));
            layout.AddAutoSized(new Label { Text = "State" });
            layout.AddAutoSized(stateList);
            //layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "City" });
            layout.AddAutoSized(cityList);
            layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Zip Code" });
            layout.AddAutoSized(zipList);
            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndGroup();

            layout.BeginGroup("Business Category", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Categories" });
            layout.AddAutoSized(catList);
            layout.BeginHorizontal();
            layout.AddAutoSized(add);
            layout.AddAutoSized(remove);
            layout.EndHorizontal();
            layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Selected" });
            layout.AddAutoSized(selectedCats);
            layout.BeginCentered();
            layout.AddAutoSized(search);
            layout.EndCentered();
            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndGroup();

            layout.EndVertical();
            layout.EndVertical();


            layout.BeginVertical(new Padding(10, 0, 0, 0));
            layout.BeginGroup("Search Results");
            layout.BeginCentered();
            layout.AddAutoSized(grid);
            layout.EndCentered();
            layout.EndGroup();
            layout.EndVertical();


            layout.EndHorizontal();
        }

        // Business class for the data.
        public class Business
        {
            public string name { get; set; }
            public string state { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
            public string bid { get; set; }
        }
    }
}
