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
        public event EventHandler<EventArgs> SelectionChanged;
        public class Business
        {
            public string name { get; set; }
            public string state { get; set; }
            public string city { get; set; }
            public string bid { get; set; }
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
            grid.SelectionChanged += new EventHandler<EventArgs>(businessWindow);
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

        public void businessWindow(object sender, EventArgs e)
        {
            Business B = grid.SelectedItem as Business;
            if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
            {
                BusinessForm bwindow = new BusinessForm(B.bid.ToString());
                bwindow.Show();
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
                string cmd = "SELECT name, state, city, business_id FROM business WHERE state = '" + stateList.SelectedValue.ToString() + "' AND city = '" + cityList.SelectedValue.ToString() + "' ORDER BY name";
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
            data.Add(new Business() { name = R.GetString(0), state = R.GetString(1), city = R.GetString(2), bid = R.GetString(3) });
        }
        protected virtual void OnSelectedValueChanged() // I think this should activate when the stateList dropdown is changed?
        {
            EventHandler<EventArgs> handler = SelectedValueChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged()
        {
            EventHandler<EventArgs> handler = SelectionChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }
        private void addColGrid() // Adds columns to the graph view
        {
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("name"), HeaderText = "Business Name", Width = 255, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("state"), HeaderText = "State", Width = 60, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("city"), HeaderText = "City", Width = 150, AutoSize = false, Resizable = false, Sortable = true, Editable = false });
            grid.Columns.Add(new GridColumn { DataCell = new TextBoxCell("bid"), Width = 0, AutoSize = false, Resizable = false, Sortable = true, Editable = false, Visible = false });
        }
        public void createUI() // Parameters for the UI Elements
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
    public partial class BusinessForm : Form
    {
        TableLayout layout = new TableLayout();
        private string bid = "";
        private string bname = "";
        private string bstate = "";
        private string bcity = "";
        private string statenum = "";
        private string citynum = "";
        public BusinessForm(string bid) // Main Form
        {
            Title = "Business Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution
            this.bid = String.Copy(bid);

            loadBusinessDetails();
            loadBusinessNums();
            createUI(bid); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout
        }
        private string connectionInfo()
        {
            return "Host=localhost; Username=postgres; Database=milestone1db; Password=mustafa";
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
                        reader.Read();
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

        private void loadBusinessDetails()
        {
            string sqlStr = "SELECT name, state, city FROM business WHERE business_id = '" + this.bid + "';";
            executeQuery(sqlStr, loadBusinessDetailsHelper);
        }

        private void loadBusinessNums()
        {
            string sqlStr1 = "SELECT count(*) from business WHERE state = (SELECT state from business WHERE business_id = '" + this.bid + "');";
            executeQuery(sqlStr1, loadBusinessNumsStateHelper);
            string sqlStr2 = "SELECT count(*) from business WHERE city = (SELECT city from business WHERE business_id = '" + this.bid + "');";
            executeQuery(sqlStr2, loadBusinessNumsCityHelper);
        }

        private void loadBusinessDetailsHelper(NpgsqlDataReader R)
        {
            bname = R.GetString(0);
            bstate = R.GetString(1);
            bcity = R.GetString(2);
        }

        private void loadBusinessNumsStateHelper(NpgsqlDataReader R)
        {
            statenum = R.GetInt32(0).ToString();
        }

        private void loadBusinessNumsCityHelper(NpgsqlDataReader R)
        {
            citynum = R.GetInt32(0).ToString();
        }
        public void createUI(string bid)
        {
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);
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
                new Label { Text = "# of Businesses in State" },
                new Label { Text = statenum }
            ));
            layout.Rows.Add(new TableRow(
                new Label { Text = "# of Businesses in City" },
                new Label { Text = citynum }
            ));
            layout.Rows.Add(new TableRow { ScaleHeight = true });
        }
    }
}
