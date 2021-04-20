using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    // Class for the business details window.
    public partial class BusinessForm : Form
    {
        // Bunch of gross variables.
        DynamicLayout layout = new DynamicLayout(); // Layout for the page
        Button addTip = new Button // Button for adding a tip
        {
            Text = "Add Tip"
        };

        // These hold the number of businesses in the state/city
        private string statenum = "";
        private string citynum = "";

        // Makes sure the tip isn't a duplicate
        private bool isDuplicate;

        // Local copy of the user and business
        private UserInfo user;
        private Business bus;

        // shared info
        SharedInfo s = new SharedInfo();

        // Textbox for adding tips
        TextBox newTip = new TextBox
        {
            PlaceholderText = "New Tip",
            Size = new Size(800, -1)
        };

        // data store for tips
        DataStoreCollection<TipInfo> data = new DataStoreCollection<TipInfo>();

        // Grid to display tips
        GridView grid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        // Event handler for clicks
        public event EventHandler<EventArgs> Click;

        // Main entry point for business window.
        public BusinessForm(Business B, UserInfo inUser) // Main Form
        {
            Title = "Business Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution
            // this.bid = String.Copy(bid);
            this.bus = B;
            this.user = inUser;

            // loadBusinessDetails(); // Loads the business name, city, and state.
            loadBusinessNums(); // Loads # of businesses in city and state.
            loadBusinessTipsHelper();

            createUI(bus.bid); // Puts everything where it belongs
            addColGrid();
            this.Content = layout; // Instantiates the layout

            // Add tip button event
            addTip.Click += new EventHandler<EventArgs>(addTipHelper);
        }

        // Queries for loading the number of businesses.
        private void loadBusinessNums()
        {
            // Gets number of businesses in the state
            string sqlStr1 = "SELECT count(*) from businessaddress WHERE businessstate = (SELECT businessstate from businessaddress WHERE businessid = '" + bus.bid + "');";
            s.executeQuery(sqlStr1, loadBusinessNumsStateHelper, true);

            // Gets number of businesses in the city
            string sqlStr2 = "SELECT count(*) from businessaddress WHERE businesscity = (SELECT businesscity from businessaddress WHERE businessid = '" + bus.bid + "');";
            s.executeQuery(sqlStr2, loadBusinessNumsCityHelper, true);
        }

        // Loads the tips into the grid
        private void loadBusinessTipsHelper()
        {
            data.Clear();
            // Query for the tips
            string sqlStr = "SELECT dateWritten, userName, likes, textWritten FROM Tip, Users WHERE Users.userID = Tip.userID AND businessID = '" + bus.bid + "' ORDER BY dateWritten;";
            s.executeQuery(sqlStr, loadBusinessTipsHelper, true);
            grid.DataStore = data;
        }

        // Helper for assigning state business numbers.
        private void loadBusinessNumsStateHelper(NpgsqlDataReader R)
        {
            statenum = R.GetInt32(0).ToString();
        }

        // Helper for assigning city business numbers.
        private void loadBusinessNumsCityHelper(NpgsqlDataReader R)
        {
            citynum = R.GetInt32(0).ToString();
        }

        // Puts the tip info into a tip class to keep the info together
        private void loadBusinessTipsHelper(NpgsqlDataReader R)
        {
            data.Add(new TipInfo()
            {
                date = (DateTime)R.GetTimeStamp(0),
                name = R.GetString(1),
                likes = R.GetInt32(2),
                text = R.GetString(3)
            });
        }

        // Adds the tips to the business!
        private void addTipHelper(object sender, EventArgs e)
        {
            // Query to see if the tip exists
            String check = @"SELECT userID, businessID, textWritten FROM Tip WHERE userID = '" + user.UserID + @"' 
            AND businessid = '" + bus.bid + "' AND textWritten = '" + newTip.Text.ToString() + "';";
            s.executeQuery(check, tipExists, true);
            if (user.UserID != null && !isDuplicate && newTip.Text.Length > 0) // Makes sure theres a real tip
            {
                // Query to insert the tip
                string cmd = @"INSERT INTO Tip (userid, businessID, dateWritten, likes, textWritten)
                    VALUES ('" + user.UserID + "', '" + bus.bid + "', '" +
                    DateTime.Now + "', 0,'" + newTip.Text.ToString() + "') ;";
                s.executeQuery(cmd, empty, false);
                loadBusinessTipsHelper();
            }
            else if (newTip.Text.Length == 0) // If it can't insert
            {
                MessageBox.Show("Cannot submit an empty tip!");
            }
            else if (isDuplicate) // etc
            {
                MessageBox.Show("That tip already exists!");
            }
            else // etc
            {
                MessageBox.Show("You must log in before you submit a tip!");
            }
        }

        // Checks to see if the tip exists
        private void tipExists(NpgsqlDataReader R)
        {
            if (R.HasRows)
            {
                isDuplicate = true;
            }
            else
            {
                isDuplicate = false;
            }
        }

        // The execute reader method requires something to handle
        // reading in the data. In this case we just wanted to insert.
        // So, there is an empty method lol.
        private void empty(NpgsqlDataReader R)
        {
            // Maybe add an easter egg?
        }

        // Required by law to be here!!! (handles events)
        protected virtual void OnClick()
        {
            EventHandler<EventArgs> handler = Click;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        // Puts all of the stuff where it belongs.
        public void createUI(string bid)
        {
            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);
            grid.Size = new Size(800, 400);

            layout.BeginVertical();


            layout.BeginGroup("Buisness Info", new Padding(10, 10, 10, 10));
            layout.AddRow(
                new Label { Text = "Business Name" },
                new TextBox { Text = bus.name, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "State" },
                new TextBox { Text = bus.state, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "City" },
                new TextBox { Text = bus.city, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "Zip" },
                new TextBox { Text = bus.zip, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "Businesses in State" },
                new TextBox { Text = statenum, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "Businesses in City" },
                new TextBox { Text = citynum, ReadOnly = true }
            );
            layout.EndGroup();

            layout.BeginGroup("Tips", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.AddAutoSized(grid);
            layout.EndHorizontal();
            layout.BeginHorizontal();
            layout.AddAutoSized(newTip);
            layout.AddAutoSized(addTip);
            layout.EndHorizontal();
            layout.EndGroup();


            layout.EndVertical();
        }

        // Populates the grid with columns
        private void addColGrid()
        {
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("date"),
                HeaderText = "Date",
                Width = 150,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("name"),
                HeaderText = "Name",
                Width = 100,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("likes"),
                HeaderText = "Likes",
                Width = 50,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("text"),
                HeaderText = "Text",
                //Width = 200,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
        }
    }
}
