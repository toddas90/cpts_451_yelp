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
        private string statenum = "";
        private string citynum = "";

        // Main entry point for business window.
        public BusinessForm(string bid) // Main Form
        {
            Title = "Business Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution
            this.bid = String.Copy(bid);

            loadBusinessDetails(); // Loads the business name, city, and state.
            loadBusinessNums(); // Loads # of businesses in city and state.
            createUI(bid); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout
        }

        // Same connection info as above.
        private string connectionInfo()
        {
            return "Host=localhost; Username=postgres; Database=milestone1db; Password=mustafa";
        }

        // Same executeQuery function as above, minus the while loop.
        // Taken straight from the video.
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

        // Query for loading the name, city, and state into the business details window.
        private void loadBusinessDetails()
        {
            string sqlStr = "SELECT name, state, city FROM business WHERE business_id = '" + this.bid + "';";
            executeQuery(sqlStr, loadBusinessDetailsHelper);
        }

        // Queries for loading the number of businesses.
        private void loadBusinessNums()
        {
            string sqlStr1 = "SELECT count(*) from business WHERE state = (SELECT state from business WHERE business_id = '" + this.bid + "');";
            executeQuery(sqlStr1, loadBusinessNumsStateHelper);
            string sqlStr2 = "SELECT count(*) from business WHERE city = (SELECT city from business WHERE business_id = '" + this.bid + "');";
            executeQuery(sqlStr2, loadBusinessNumsCityHelper);
        }

        // Helper for assigning business details.
        private void loadBusinessDetailsHelper(NpgsqlDataReader R)
        {
            bname = R.GetString(0);
            bstate = R.GetString(1);
            bcity = R.GetString(2);
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

        // Puts all of the stuff where it belongs.
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