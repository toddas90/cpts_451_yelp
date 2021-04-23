using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

// TODO:


namespace cpts_451_yelp
{
    // Main window class.
    public partial class MainForm : Form
    {
        // Lots of variables, kinda gross.

        // Layout of the page.
        DynamicLayout layout = new DynamicLayout();

        // I tried adding tabs but I couldn't figure out how to
        // put the content inside the tabs.

        // TabControl tabs = new TabControl();
        // TabPage businessTab = new TabPage
        // {
        //     Text = "Business Info"
        // };
        // TabPage UserTab = new TabPage
        // {
        //     Text = "User Info"
        // };

        // Labels for number of businesses
        Label stnum = new Label();
        Label ctnum = new Label();

        // List of the states, pupulated vis sql
        DropDown stateList = new DropDown();

        // List of cities, populated after states
        ListBox cityList = new ListBox
        {
            Size = new Size(150, 100)
        };

        // List of zip codes, populated after cities
        ListBox zipList = new ListBox
        {
            Size = new Size(150, 100)
        };

        // List of possible categories to narrow search
        ListBox catList = new ListBox
        {
            Size = new Size(150, 100)
        };

        // List of all the selected categories
        ListBox selectedCats = new ListBox
        {
            Size = new Size(150, 100)
        };

        ListBox selectedAtts = new ListBox
        {
            Size = new Size(150, 100)
        };

        ListBox priceFilters = new ListBox
        {
            Size = new Size(150, 100)
        };

        ListBox attributeFilters = new ListBox
        {
            Size = new Size(150, 100)
        };

        ListBox selectedFilters = new ListBox
        {
            Size = new Size(150, 100)
        };

        ListBox mealFilters = new ListBox
        {
            Size = new Size(150, 100)
        };

        // Add selected categories to list
        Button add_cat = new Button
        {
            Text = "Add"
        };

        Button add_att = new Button
        {
            Text = "Add"
        };

        // Search using the selected categories
        Button search = new Button
        {
            Text = "Search"
        };

        // Remove category from selectedCats
        Button remove_cat = new Button
        {
            Text = "Remove"
        };

        Button remove_att = new Button
        {
            Text = "Remove"
        };

        // Opens the user page, login and user info live here
        Button user = new Button
        {
            Text = "User"
        };

        // Grid for displaying the businesses
        GridView grid = new GridView<Business>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true,
            AllowColumnReordering = false
        };

        Dictionary<string, string> attMoneyPairs = new Dictionary<string, string>();
        Dictionary<string, string> attRegPairs = new Dictionary<string, string>();
        Dictionary<string, string> attMealPairs = new Dictionary<string, string>();

        // Info about the currently logged in user
        UserInfo currentUser = new UserInfo();

        // Some info that is used in multiple places.
        // I don't really know C# at all so this is 100%
        // a terrible way of doing it. But it works.
        SharedInfo s = new SharedInfo();

        // Creates a DataStore for the grid. This is how rows work I guess.
        DataStoreCollection<Business> data = new DataStoreCollection<Business>();

        // Event when something in a combobox is selected
        public event EventHandler<EventArgs> SelectedValueChanged;

        // Event handler for the grid selection event.
        public event EventHandler<EventArgs> SelectionChanged;

        // Event for when buttons are clicked
        public event EventHandler<EventArgs> Click;


        // Main Form where everything happens
        public MainForm()
        {
            Title = "Yelp App"; // Title of Application
            MinimumSize = new Size(1600, 900); // Default resolution

            currentUser.UserID = "/0";

            createUI(); // Puts everything where it belongs
            addColGrid(); // Creates the data grid
            this.Content = layout; // Instantiates the layout
            queryState(); // Put states in drop down
            populateFilters(); // Adds the filters to the lists


            // These attach the event handlers to the specific functions.
            // ie when a value in the stateList is changes, it calls queryCity.
            stateList.SelectedValueChanged += new EventHandler<EventArgs>(queryCity);
            cityList.SelectedValueChanged += new EventHandler<EventArgs>(queryZip);
            zipList.SelectedValueChanged += new EventHandler<EventArgs>(queryCat);
            zipList.SelectedValueChanged += new EventHandler<EventArgs>(queryBusiness);
            add_cat.Click += new EventHandler<EventArgs>(addSelectedCat);
            remove_cat.Click += new EventHandler<EventArgs>(removeSelectedCat);
            add_att.Click += new EventHandler<EventArgs>(addSelectedAtt);
            remove_att.Click += new EventHandler<EventArgs>(removeSelectedAtt);
            search.Click += new EventHandler<EventArgs>(queryBusiness);
            grid.SelectionChanged += new EventHandler<EventArgs>(businessWindow);
            user.Click += new EventHandler<EventArgs>(userWindow);
        }

        // Creates the Business Details Window and passes along the business id of the
        // business that was clicked on.
        public void businessWindow(object sender, EventArgs e)
        {
            if (grid.SelectedItem == null) // Checks for null value
            {
                return;
            }
            Business B = grid.SelectedItem as Business;
            // Console.WriteLine("Hello from " + B.name); // For debugging!
            try
            {
                if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
                {
                    BusinessForm bwindow = new BusinessForm(
                        B,
                        //B.bid.ToString(),
                        currentUser
                    ); // Creates the new business window
                    bwindow.Show(); // Displays the new window
                }
            }
            catch (System.InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
        }

        // Creates the user page. This is where the user login is located
        public void userWindow(object sender, EventArgs e)
        {
            try
            {
                userForm uwindow = new userForm(); // Creates a new user page
                uwindow.Show(); // Displays the page

                // Sets the user in here to the one selected in the user page
                currentUser = uwindow.currentUser;
            }
            catch (System.InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
        }


        // Queries for loading the number of businesses.
        private void loadBusinessNumsState()
        {
            // Gets number of businesses in the state
            string sqlStr1 = "SELECT count(*) from businessaddress WHERE businessstate = '" + stateList.SelectedValue.ToString() + "';";
            s.executeQuery(sqlStr1, loadBusinessNumsStateHelper, true);
        }

        private void loadBusinessNumsCity()
        {
            if (cityList.SelectedIndex > -1)
            {
                // Gets number of businesses in the city
                string sqlStr2 = "SELECT count(*) from businessaddress WHERE businesscity = '" + cityList.SelectedValue.ToString() + "';";
                s.executeQuery(sqlStr2, loadBusinessNumsCityHelper, true);
            }
        }

        // Helper for assigning state business numbers.
        private void loadBusinessNumsStateHelper(NpgsqlDataReader R)
        {
            stnum.Text = R.GetInt32(0).ToString();
        }

        // Helper for assigning city business numbers.
        private void loadBusinessNumsCityHelper(NpgsqlDataReader R)
        {
            ctnum.Text = R.GetInt32(0).ToString();
        }

        public void populateFilters()
        {
            priceFilters.Items.Add("$");
            priceFilters.Items.Add("$$");
            priceFilters.Items.Add("$$$");
            priceFilters.Items.Add("$$$$");

            attributeFilters.Items.Add("Accepts Credit Cards");
            attributeFilters.Items.Add("Takes Reservations");
            attributeFilters.Items.Add("Wheelchair Accessable");
            attributeFilters.Items.Add("Outdoor Seating");
            attributeFilters.Items.Add("Good for Kids");
            attributeFilters.Items.Add("Good for Groups");
            attributeFilters.Items.Add("Delivery");
            attributeFilters.Items.Add("Takeout");
            attributeFilters.Items.Add("Free Wifi");
            attributeFilters.Items.Add("Bike Parking");

            mealFilters.Items.Add("Breakfast");
            mealFilters.Items.Add("Brunch");
            mealFilters.Items.Add("Lunch");
            mealFilters.Items.Add("Dinner");
            mealFilters.Items.Add("Dessert");
            mealFilters.Items.Add("Late Night");
        }

        // This queries the db for the states.
        public void queryState()
        {
            // Clears the grid data and the other boxes.
            cityList.Items.Clear();
            zipList.Items.Clear();
            catList.Items.Clear();
            selectedCats.Items.Clear();
            data.Clear();

            // Query for the business box
            string cmd = @"SELECT distinct businessstate FROM businessaddress
                ORDER BY businessstate";
            s.executeQuery(cmd, queryStateHelper, true);
        }

        // This queries the db for the cities.
        public void queryCity(object sender, EventArgs e)
        {
            // Again, clears the grid data and the boxes.
            cityList.Items.Clear();
            zipList.Items.Clear();
            catList.Items.Clear();
            selectedCats.Items.Clear();
            data.Clear();
            ctnum.Text = "0";

            loadBusinessNumsState();

            if (stateList.SelectedIndex > -1) // Checks to see if a state has actually been selected
            {
                // The query for the city box
                string cmd = @"SELECT distinct businesscity FROM businessaddress
                    WHERE businessstate = '" +
                    stateList.SelectedValue.ToString() +
                    "' ORDER BY businesscity";
                s.executeQuery(cmd, queryCityHelper, true);
            }
        }

        public void queryZip(object sender, EventArgs e)
        {
            // Again, clears the grid data and the boxes.
            zipList.Items.Clear();
            catList.Items.Clear();
            selectedCats.Items.Clear();
            data.Clear();

            loadBusinessNumsCity();

            if (cityList.SelectedIndex > -1) // Checks to see if a city has been selected
            {
                // Query for the zip box
                string cmd = @"SELECT distinct businesspostalcode FROM 
                    businessaddress WHERE businessstate = '" +
                    stateList.SelectedValue.ToString() + @"' AND 
                    businesscity = '" + cityList.SelectedValue.ToString() +
                    "' ORDER BY businesspostalcode";
                s.executeQuery(cmd, queryZipHelper, true);
            }
        }

        public void queryCat(object sender, EventArgs e)
        {
            // Again, clears the grid data and the boxes.
            catList.Items.Clear();
            selectedCats.Items.Clear();
            data.Clear();

            if (zipList.SelectedIndex > -1) // Checks for selected item in the ziplist
            {
                // Fills up the categories box
                string cmd = @"SELECT DISTINCT categoryname FROM categories,
                    businessaddress, business WHERE categories.businessid = 
                    business.businessid AND business.businessid = 
                    businessaddress.businessid AND businessstate = '" +
                    stateList.SelectedValue.ToString() + @"' AND businesscity =
                    '" + cityList.SelectedValue.ToString() + @"' AND 
                    businesspostalcode = '" + zipList.SelectedValue.ToString()
                    + "' ORDER BY categoryname";
                s.executeQuery(cmd, queryCatHelper, true);
            }
        }

        public void queryBusiness(object sender, EventArgs e)
        {
            if (selectedAtts.Items.Count > 0)
            {
                stringifyAttributes(selectedAtts.Items);
            }
            data.Clear(); // Clears the data
            if (selectedCats.Items.Count > 0) // Checks if there are selected categories
            {
                // Query to run if categories have been selected
                string cmd = @"SELECT DISTINCT businessname, businessstate,
                    businesscity, businesspostalcode, business.businessid, 
                    businessstreetaddress, stars, tipcount, checkincount 
                    FROM businessaddress, business, categories, (SELECT 
                    DISTINCT  businessID, COUNT(businessID) 
                    as count FROM categories WHERE " +
                    stringifyCategories(selectedCats.Items)
                    + @" GROUP BY businessID) as num WHERE categories.businessid
                    = business.businessid AND business.businessid = 
                    businessaddress.businessid AND categories.businessid = 
                    businessaddress.businessid AND business.businessID = 
                    num.businessid AND num.count = '" + selectedCats.Items.Count
                    + "' AND businessstate = '" +
                    stateList.SelectedValue.ToString() + @"' AND businesscity = 
                    '" + cityList.SelectedValue.ToString() + @"' AND 
                    businesspostalcode = '" + zipList.SelectedValue.ToString()
                    + @"' ORDER BY businessname";
                s.executeQuery(cmd, queryBusinessHelper, true);
                grid.DataStore = data; // Sets the data in the grid
            }
            else if (zipList.SelectedIndex > -1) // Query to run normally, checks if a zip has been selected
            {
                // Query to populate businesses in the grid from a zip
                string cmd = @"SELECT DISTINCT businessname, businessstate, 
                    businesscity, businesspostalcode, business.businessid, 
                    businessstreetaddress, stars, tipcount, checkincount FROM 
                    businessaddress, business WHERE business.businessid = 
                    businessaddress.businessid AND businessstate = '" +
                    stateList.SelectedValue.ToString() + @"' AND businesscity = 
                    '" + cityList.SelectedValue.ToString() + @"' AND 
                    businesspostalcode = '" + zipList.SelectedValue.ToString() +
                    "' ORDER BY businessname";
                s.executeQuery(cmd, queryBusinessHelper, true);
                grid.DataStore = data; // Sets data in grid
            }
        }

        // As the name suggests, it turns the list of categories into a string
        // so they can be used inside the query
        public string stringifyCategories(ListItemCollection lst)
        {
            string ret = " categoryname =" + "'" + lst[0].ToString() + "'";
            for (int i = 1; i < lst.Count; i++)
            {
                ret += " OR categoryname = " + "'" + lst[i].ToString() + "'";
            }
            return ret;
        }

        //turns the attributes into a string usable in the business query 
        public string stringifyAttributes(ListItemCollection lst)
        {
            string ret = "";
            // if(/*regular/meal attribute is selected*/)
            // {
            //     ret = "attributename ='" + mapAttributes(lst[0].ToString());
            //     for (int i = 1; i < reg.Count; i++)
            //     {
            //         trimmednextitem = reg[i].ToString().Replace(" ","");
            //         ret += " OR attributename = " + "'" + trimmednextitem + "'";
            //     }
            // }
            // if (/*money attribute is selected*/)
            // {

            // }

            for (int i = 0; i < lst.Count; i++)
            {
                MessageBox.Show(mapAttributes(lst[i].ToString()));
            }

            return ret;
        }

        // Gross method to map names in list to the actual
        // Attribute name in the db. We tried other things
        // but they ended up not working right. Time crunch!
        public string mapAttributes(string displayname)
        {
            displayname = displayname.Replace(" ", "");

            if (displayname.ToLower() == "acceptscreditcards")
            {
                return "BusinessAcceptsCreditCards";
            }
            else if (displayname.ToLower() == "takesreservations")
            {
                return "RestaurantsReservations";
            }
            else if (displayname.ToLower() == "wheelchairaccessible")
            {
                return "WheelchairAccessible";
            }
            else if (displayname.ToLower() == "outdoorseating")
            {
                return "OutdoorSeating";
            }
            else if (displayname.ToLower() == "goodforkids")
            {
                return "GoodForKids";
            }
            else if (displayname.ToLower() == "goodforgroups")
            {
                return "GoodForGroups";
            }
            else if (displayname.ToLower() == "delivery")
            {
                return "RestaurantsDelivery";
            }
            else if (displayname.ToLower() == "takeout")
            {
                return "RestaurantsTakeout";
            }
            else if (displayname.ToLower() == "freewifi")
            {
                return "WiFi";
            }
            else if (displayname.ToLower() == "bikeparking")
            {
                return "BikeParking";
            }
            else if (displayname.ToLower() == "breakfast")
            {
                return "GoodForMeal:breakfast";
            }
            else if (displayname.ToLower() == "brunch")
            {
                return "GoodForMeal:brunch";
            }
            else if (displayname.ToLower() == "lunch")
            {
                return "GoodForMeal:lunch";
            }
            else if (displayname.ToLower() == "dinner")
            {
                return "GoodForMeal:dinner";
            }
            else if (displayname.ToLower() == "dessert")
            {
                return "GoodForMeal:dessert";
            }
            else if (displayname.ToLower() == "latenight")
            {
                return "GoodForMeal:latenight";
            }
            else if (displayname.ToLower() == "$")
            {
                return "1";
            }
            else if (displayname.ToLower() == "$$")
            {
                return "2";
            }
            else if (displayname.ToLower() == "$$$")
            {
                return "3";
            }
            else if (displayname.ToLower() == "$$$$")
            {
                return "4";
            }
            else
            {
                return "";
            }

        }

        // Happens when add is clicked.
        // Adds the selected object to the categories list.
        public void addSelectedCat(object sender, EventArgs e)
        {
            for (int i = 0; i < selectedCats.Items.Count; i++)
            {
                if (selectedCats.Items[i].ToString() == catList.SelectedValue.ToString())
                {
                    // Console.WriteLine("Item found"); // For debugging
                    return; // If the item is already in the list, it will not add it again
                }
            }

            try
            {
                // Tries to add the selected category to the list.
                // Fails if there is nothing selected.
                selectedCats.Items.Add(catList.SelectedValue.ToString());
                catList.SelectedIndex = -1;
            }
            catch (System.NullReferenceException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                // MessageBox.Show("Please select a category first!");
            }
        }

        // Happens when add is clicked.
        // Adds the selected object to the categories list.
        public void addSelectedAtt(object sender, EventArgs e)
        {
            try
            {
                // Tries to add the selected category to the list.
                // Fails if there is nothing selected.
                if (priceFilters.SelectedIndex > -1)
                {
                    for (int i = 0; i < selectedAtts.Items.Count; i++)
                    {
                        if (selectedAtts.Items[i].ToString() == priceFilters.SelectedValue.ToString())
                        {
                            // Console.WriteLine("Item found"); // For debugging
                            return; // If the item is already in the list, it will not add it again
                        }
                    }
                    selectedAtts.Items.Add(priceFilters.SelectedValue.ToString());
                    priceFilters.SelectedIndex = -1;
                }
                if (attributeFilters.SelectedIndex > -1)
                {
                    for (int i = 0; i < selectedAtts.Items.Count; i++)
                    {
                        if (selectedAtts.Items[i].ToString() == attributeFilters.SelectedValue.ToString())
                        {
                            // Console.WriteLine("Item found"); // For debugging
                            return; // If the item is already in the list, it will not add it again
                        }
                    }
                    selectedAtts.Items.Add(attributeFilters.SelectedValue.ToString());
                    attributeFilters.SelectedIndex = -1;
                }
                if (mealFilters.SelectedIndex > -1)
                {
                    for (int i = 0; i < selectedAtts.Items.Count; i++)
                    {
                        if (selectedAtts.Items[i].ToString() == mealFilters.SelectedValue.ToString())
                        {
                            // Console.WriteLine("Item found"); // For debugging
                            return; // If the item is already in the list, it will not add it again
                        }
                    }
                    selectedAtts.Items.Add(mealFilters.SelectedValue.ToString());
                    mealFilters.SelectedIndex = -1;
                }
            }
            catch (System.NullReferenceException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                // MessageBox.Show("Please select an attribute first!");
            }
        }

        // Removes the selected item from the list.
        public void removeSelectedCat(object sender, EventArgs e)
        {
            try
            {
                // Tries to remove the selected item.
                // fails if nothing is selected.
                selectedCats.Items.RemoveAt(selectedCats.SelectedIndex);
                selectedCats.SelectedIndex = -1;
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                // MessageBox.Show("Please select a category first!");
            }
        }


        // Removes the selected item from the list.
        public void removeSelectedAtt(object sender, EventArgs e)
        {
            try
            {
                // Tries to remove the selected item.
                // fails if nothing is selected.
                selectedAtts.Items.RemoveAt(selectedAtts.SelectedIndex);
                selectedAtts.SelectedIndex = -1;
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message.ToString());
                // MessageBox.Show("Please select an attribute first!");
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

        // Function that adds the zips to the list.
        private void queryZipHelper(NpgsqlDataReader R)
        {
            zipList.Items.Add(R.GetString(0));
        }

        // Function that adds the categories to the list.
        private void queryCatHelper(NpgsqlDataReader R)
        {
            catList.Items.Add(R.GetString(0));
        }

        // Function that adds the businesses to the grid data store.
        private void queryBusinessHelper(NpgsqlDataReader R)
        {
            data.Add(new Business() // Made a business class to keep the info together
            {
                // This just populates the info into an instance of the business class
                // These have to be in the same order as the select statement in the query
                // or else things break!
                name = R.GetString(0), // name
                state = R.GetString(1), // state
                city = R.GetString(2), // city
                zip = R.GetString(3), // zip
                bid = R.GetString(4), // business id
                addy = R.GetString(5), // address
                // dist = R.GetDouble(6), // distance from user
                stars = R.GetDouble(6), // number of stars
                tips = R.GetInt32(7), // number of tips
                checkins = R.GetInt32(8) // number of check ins
            });
        }

        // Used in the event handling for the DropDown menus. Needs to be here.
        protected virtual void OnSelectedValueChanged()
        {
            EventHandler<EventArgs> handler = SelectedValueChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        // Used in the event handling for the Grid selection. Needs to be here.
        protected virtual void OnSelectionChanged()
        {
            EventHandler<EventArgs> handler = SelectionChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        // Used for click event handling (buttons)
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
                //Width = 300,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("addy"),
                HeaderText = "Address",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("city"),
                HeaderText = "City",
                //Width = 120,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("state"),
                HeaderText = "State",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("dist"),
                HeaderText = "Distance",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("stars"),
                HeaderText = "Stars",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("tips"),
                HeaderText = "Tips",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("checkins"),
                HeaderText = "Check-ins",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
        }

        // Creates and places all of the UI elements. This is particularly
        // nasty looking. There is probably a better way to do this, but idk.
        // https://github.com/picoe/Eto/wiki/DynamicLayout
        public void createUI()
        {
            layout.Padding = new Padding(10, 0, 10, 10);
            grid.Size = new Size(1000, 1000);
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
            layout.AddAutoSized(new Label { Text = "Businesses in State" });
            layout.AddAutoSized(stnum);
            layout.AddAutoSized(new Label { Text = "Businesses in City" });
            layout.AddAutoSized(ctnum);
            layout.EndVertical();
            layout.EndHorizontal();

            layout.EndGroup();

            layout.BeginHorizontal();
            layout.BeginGroup("User Info", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(user);
            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndGroup();
            layout.EndHorizontal();

            layout.EndVertical();

            layout.BeginVertical(new Padding(10, 0, 0, 0));
            layout.BeginGroup("Search Results");
            layout.BeginCentered();
            layout.AddAutoSized(grid);
            layout.EndCentered();
            layout.EndGroup();
            layout.EndVertical();

            layout.BeginGroup("Filters", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Price" });
            layout.AddAutoSized(priceFilters);
            layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Attributes" });
            layout.AddAutoSized(attributeFilters);
            layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Meal" });
            layout.AddAutoSized(mealFilters);
            layout.BeginHorizontal();
            layout.AddAutoSized(add_att);
            layout.AddAutoSized(remove_att);
            layout.EndHorizontal();
            layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Selected Attributes" });
            layout.AddAutoSized(selectedAtts);
            layout.BeginCentered();
            layout.AddAutoSized(search);
            layout.EndCentered();
            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndGroup();

            layout.BeginGroup("Categories", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Category" });
            layout.AddAutoSized(catList);
            layout.BeginHorizontal();
            layout.AddAutoSized(add_cat);
            layout.AddAutoSized(remove_cat);
            layout.EndHorizontal();
            layout.EndVertical();
            layout.EndHorizontal();

            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Selected Categories" });
            layout.AddAutoSized(selectedCats);
            layout.BeginCentered();
            layout.AddAutoSized(search);
            layout.EndCentered();
            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndGroup();

            layout.EndHorizontal();
        }
    }
}
