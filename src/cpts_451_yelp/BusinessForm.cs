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
        private string bid = "";
        private string bname = "";
        private string bstate = "";
        private string bcity = "";
        private string bzip = "";
        private string statenum = "";
        private string citynum = "";

        private bool isDuplicate;

        private UserInfo user;

        SharedInfo s = new SharedInfo();

        TextBox newTip = new TextBox
        {
            PlaceholderText = "New Tip",
            Size = new Size(800, -1)
        };

        DataStoreCollection<TipInfo> data = new DataStoreCollection<TipInfo>();

        GridView grid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        public event EventHandler<EventArgs> Click;

        // Main entry point for business window.
        public BusinessForm(string bid, UserInfo inUser) // Main Form
        {
            Title = "Business Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution
            this.bid = String.Copy(bid);
            this.user = inUser;

            loadBusinessDetails(); // Loads the business name, city, and state.
            loadBusinessNums(); // Loads # of businesses in city and state.
            loadBusinessTipsHelper();

            createUI(bid); // Puts everything where it belongs
            addColGrid();
            this.Content = layout; // Instantiates the layout

            addTip.Click += new EventHandler<EventArgs>(addTipHelper);
        }

        // Query for loading the name, city, state, and zip into the business details window.
        private void loadBusinessDetails()
        {
            data.Clear();
            string sqlStr = "SELECT businessname, businessstate, businesscity, businesspostalcode FROM businessaddress, business WHERE business.businessid = businessaddress.businessid AND businessaddress.businessid = '" + this.bid + "';";
            s.executeQuery(sqlStr, loadBusinessDetailsHelper, true);
        }

        // Queries for loading the number of businesses.
        private void loadBusinessNums()
        {
            string sqlStr1 = "SELECT count(*) from businessaddress WHERE businessstate = (SELECT businessstate from businessaddress WHERE businessid = '" + this.bid + "');";
            s.executeQuery(sqlStr1, loadBusinessNumsStateHelper, true);
            string sqlStr2 = "SELECT count(*) from businessaddress WHERE businesscity = (SELECT businesscity from businessaddress WHERE businessid = '" + this.bid + "');";
            s.executeQuery(sqlStr2, loadBusinessNumsCityHelper, true);
        }

        private void loadBusinessTipsHelper()
        {
            data.Clear();
            string sqlStr = "SELECT dateWritten, userName, likes, textWritten FROM Tip, Users WHERE Users.userID = Tip.userID AND businessID = '" + this.bid + "' ORDER BY dateWritten;";
            s.executeQuery(sqlStr, loadBusinessTipsHelper, true);
            grid.DataStore = data;
        }

        // Helper for assigning business details.
        private void loadBusinessDetailsHelper(NpgsqlDataReader R)
        {
            bname = R.GetString(0);
            bstate = R.GetString(1);
            bcity = R.GetString(2);
            bzip = R.GetString(3);
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

        private void addTipHelper(object sender, EventArgs e)
        {
            String check = @"SELECT userID, businessID, textWritten FROM Tip WHERE userID = '" + user.UserID + @"' 
            AND businessid = '" + this.bid + "' AND textWritten = '" + newTip.Text.ToString() + "';";
            s.executeQuery(check, tipExists, true);
            if (user.UserID != null && !isDuplicate && newTip.Text.Length > 0)
            {

                string cmd = @"INSERT INTO Tip (userid, businessID, dateWritten, likes, textWritten)
                    VALUES ('" + user.UserID + "', '" + this.bid + "', '" +
                    DateTime.Now + "', 0,'" + newTip.Text.ToString() + "') ;";
                s.executeQuery(cmd, empty, false);
                loadBusinessTipsHelper();
            }
            else if (newTip.Text.Length == 0)
            {
                MessageBox.Show("Cannot submit an empty tip!");
            }
            else if (isDuplicate)
            {
                MessageBox.Show("That tip already exists!");
            }
            else
            {
                MessageBox.Show("You must log in before you submit a tip!");
            }
        }

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

        private void empty(NpgsqlDataReader R)
        {

        }

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
                new TextBox { Text = bname, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "State" },
                new TextBox { Text = bstate, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "City" },
                new TextBox { Text = bcity, ReadOnly = true }
            );
            layout.AddRow(
                new Label { Text = "Zip" },
                new TextBox { Text = bzip, ReadOnly = true }
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
