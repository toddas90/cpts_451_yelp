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

        Button addLike = new Button // Button for adding a like
        {
            Text = "Like 👍"
        };

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
        DataStoreCollection<TipInfo> general_data = new DataStoreCollection<TipInfo>();

        // Data for friend tips
        DataStoreCollection<TipInfo> friend_data = new DataStoreCollection<TipInfo>();

        // Grid to display tips
        GridView general_grid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        GridView friend_grid = new GridView<TipInfo>
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
            loadBusinessTipsHelper();

            createUI(bus.bid); // Puts everything where it belongs
            addColGrid(general_grid);
            addColGrid(friend_grid);
            this.Content = layout; // Instantiates the layout

            // Add tip button event
            addTip.Click += new EventHandler<EventArgs>(addTipHelper);
            // like button event
            addLike.Click += new EventHandler<EventArgs>(addLikeHelper);
        }

        // Loads the tips into the grid
        private void loadBusinessTipsHelper()
        {
            general_data.Clear();
            friend_data.Clear();
            // Query for the tips
            string sqlStr1 = @"SELECT dateWritten, userName, likes, textWritten FROM Tip, Users 
                            WHERE Users.userID = Tip.userID AND businessID = '" + bus.bid + "' ORDER BY dateWritten;";
            s.executeQuery(sqlStr1, loadBusinessTipsHelper, true);
            general_grid.DataStore = general_data;

            if (user.UserID != "/0")
            {
                // Query friend tips for the business
                string sqlStr2 = @"SELECT dateWritten, userName, likes, textWritten 
                                FROM Tip, Users, FriendsWith WHERE Users.userID = Tip.userID AND FriendsWith.UserID = '" + user.UserID + @"'
                                AND FriendsWith.FriendID = Tip.UserID AND businessID = '" + bus.bid + "' ORDER BY dateWritten;";
                s.executeQuery(sqlStr2, loadBusinessFriendTipsHelper, true);
                friend_grid.DataStore = friend_data;
            }
        }

        // Puts the tip info into a tip class to keep the info together
        private void loadBusinessTipsHelper(NpgsqlDataReader R)
        {
            general_data.Add(new TipInfo()
            {
                date = (DateTime)R.GetTimeStamp(0),
                name = R.GetString(1),
                likes = R.GetInt32(2),
                text = R.GetString(3)
            });
        }

        // Hate duplicate code, but I couldn't figure out how to make it accept
        // a data store using her reader method.
        private void loadBusinessFriendTipsHelper(NpgsqlDataReader R)
        {
            friend_data.Add(new TipInfo()
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
            // Query to see if the tip already exists
            String check = @"SELECT userID, businessID, textWritten FROM Tip WHERE userID = '" + user.UserID + @"' 
            AND businessid = '" + bus.bid + "' AND textWritten = '" + newTip.Text.ToString() + "';";
            s.executeQuery(check, tipExists, true);
            if (!isDuplicate && newTip.Text.Length > 0 && user.UserID != "/0") // Makes sure theres a real tip
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

        // Adds a like to a tip
        private void addLikeHelper(object sender, EventArgs e)
        {
            if (general_grid.SelectedItem == null) // Checks for null value
            {
                MessageBox.Show("You must select a tip to like!");
                return;
            }

            TipInfo temp = general_grid.SelectedItem as TipInfo; // Puts the thing in a tip that I can access

            if (temp.name != null && user.UserID != "/0") // If the tip has been selected and the user is logged in
            {
                Console.WriteLine("Like +1");
                // // Query to insert the like
                // string cmd = @"INSERT INTO Tip (userid, businessID, dateWritten, likes, textWritten)
                //     VALUES ('" + user.UserID + "', '" + bus.bid + "', '" +
                //     DateTime.Now + "', 0,'" + newTip.Text.ToString() + "') ;";
                // s.executeQuery(cmd, empty, false);
                // loadBusinessTipsHelper();
            }
            else // User has to be logged in
            {
                MessageBox.Show("You must log in to like a tip!");
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
            // Use as a placeholder in executeQuery for inserting
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
            general_grid.Size = new Size(800, 400);
            friend_grid.Size = new Size(800, 100);

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
            layout.EndGroup();

            layout.BeginGroup("Friend Tips", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.AddAutoSized(friend_grid);
            layout.EndHorizontal();
            layout.EndGroup();

            layout.BeginGroup("Tips", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.AddAutoSized(general_grid);

            layout.BeginVertical();
            layout.AddAutoSized(addLike);
            layout.EndVertical();

            layout.EndHorizontal();
            layout.BeginHorizontal();
            layout.AddAutoSized(newTip);
            layout.AddAutoSized(addTip);
            layout.EndHorizontal();
            layout.EndGroup();


            layout.EndVertical();
        }

        // Populates the grid with columns
        private void addColGrid(Grid T)
        {
            T.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("date"),
                HeaderText = "Date",
                Width = 150,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            T.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("name"),
                HeaderText = "Name",
                Width = 100,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            T.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("likes"),
                HeaderText = "Likes",
                Width = 50,
                AutoSize = false,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            T.Columns.Add(new GridColumn
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
