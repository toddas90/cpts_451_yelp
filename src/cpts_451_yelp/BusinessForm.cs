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
        TableLayout layout = new TableLayout();
        private string bid = "";
        private string bname = "";
        private string bstate = "";
        private string bcity = "";
        private string bzip = "";
        private string statenum = "";
        private string citynum = "";

        private UserInfo user;

        SharedInfo s = new SharedInfo();

        DataStoreCollection<TipInfo> data = new DataStoreCollection<TipInfo>();

        GridView grid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

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
        }

        // Query for loading the name, city, state, and zip into the business details window.
        private void loadBusinessDetails()
        {
            data.Clear();
            string sqlStr = "SELECT businessname, businessstate, businesscity, businesspostalcode FROM businessaddress, business WHERE business.businessid = businessaddress.businessid AND businessaddress.businessid = '" + this.bid + "';";
            s.executeQuery(sqlStr, loadBusinessDetailsHelper);
        }

        // Queries for loading the number of businesses.
        private void loadBusinessNums()
        {
            string sqlStr1 = "SELECT count(*) from businessaddress WHERE businessstate = (SELECT businessstate from businessaddress WHERE businessid = '" + this.bid + "');";
            s.executeQuery(sqlStr1, loadBusinessNumsStateHelper);
            string sqlStr2 = "SELECT count(*) from businessaddress WHERE businesscity = (SELECT businesscity from businessaddress WHERE businessid = '" + this.bid + "');";
            s.executeQuery(sqlStr2, loadBusinessNumsCityHelper);
        }

        private void loadBusinessTipsHelper()
        {
            string sqlStr = "SELECT dateWritten, userName, likes, textWritten FROM Tip, Users WHERE Users.userID = Tip.userID AND businessID = '" + this.bid + "' ORDER BY dateWritten;";
            s.executeQuery(sqlStr, loadBusinessTipsHelper);
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

        // Puts all of the stuff where it belongs.
        public void createUI(string bid)
        {
            // Recreate the UI using a DynamicLayout instead of TableLayout.
            // Maybe use an if to see if there is a user selected. If so, add an "Add Tip" button and textbox.
            // If not, don't. Just let them view tips, like now.
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);
            grid.Size = new Size(500, 500);

            layout.Rows.Add(new TableRow(
                new Label { Text = "Business Name" },
                new TextBox { Text = bname, ReadOnly = true }
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "State" },
                new TextBox { Text = bstate, ReadOnly = true }
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "City" },
                new TextBox { Text = bcity, ReadOnly = true }
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "Zip" },
                new TextBox { Text = bzip, ReadOnly = true }
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "# of Businesses in State" },
                new Label { Text = statenum }
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "# of Businesses in City" },
                new Label { Text = citynum }
            ));
            layout.Rows.Add(new TableRow(
                TableLayout.AutoSized(grid)
            ));
            layout.Rows.Add(new TableRow { ScaleHeight = true });
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
                Width = 200,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
        }
    }
}
