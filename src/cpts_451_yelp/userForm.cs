using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    // Class for the user details window.
    public partial class userForm : Form
    {
        // Click event handler
        public event EventHandler<EventArgs> Click;

        // Selection event handler for boxes
        public event EventHandler<EventArgs> SelectedValueChanged;

        // Data store for the tips info
        DataStoreCollection<UserInfo> friendData = new DataStoreCollection<UserInfo>();

        // Grid for friends of the user
        GridView friendsGrid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        // Bunch of gross variables.
        DynamicLayout layout = new DynamicLayout(); // Layout for the page
        TextBox nameBox = new TextBox // For user to log-in
        {
            PlaceholderText = "Name"
        };

        ListBox nameList = new ListBox // Box of searched names
        {
            Size = new Size(150, 100)
        };

        Button search = new Button // button to search for name
        {
            Text = "Search"
        };
        SharedInfo s = new SharedInfo(); // Shared info

        public UserInfo currentUser = new UserInfo(); // To store user information

        // Main entry point for user window.
        public userForm() // Main Form
        {
            Title = "User Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution

            createUI(); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout

            // Events are attached to event handlers here
            search.Click += new EventHandler<EventArgs>(queryName);
            nameList.SelectedValueChanged += new EventHandler<EventArgs>(setUser);
        }

        // Queries the userid based on the name entered
        public void queryName(object sender, EventArgs e)
        {
            nameList.Items.Clear(); // Clears the box

            // Query to select userid
            string cmd = @"SELECT Users.userid, username, latitude, longitude FROM Users INNER JOIN UserLocation ON Users.userid = UserLocation.userid 
                        INNER JOIN UserRating ON UserLocation.userid = UserRating.userid WHERE username = '" + nameSearch() + "'";
            s.executeQuery(cmd, queryNameHelper, true);
        }

        public void queryFriends()
        {
            friendData.Clear();

            string cmd = @"SELECT friendid, username, likes, averageStars, totalLikes, yelpingSince 
                        FROM FriendsWith, Users WHERE FriendsWith.userID = Users.UserID AND FriendsWith.UserId = '" + currentUser.UserID + "' ;";
            s.executeQuery(cmd, queryFriendInfoHelper, true);
        }

        private void queryFriendInfoHelper(NpgsqlDataReader R)
        {
            friendData.Add(new UserInfo() // Made a business class to keep the info together
            {
                UserID = R.GetString(0),
                Username = R.GetString(1), // name
                likes = R.GetInt32(2), // total likes
                avgStars = (double) R.GetDecimal(3), //average stars
                date = R.GetDateTime(4) // yelping since date
            });
        }

        // Converts the text to a string
        public String nameSearch()
        {
            return nameBox.Text.ToString();
        }

        // Sets the user in userinfo depending on which
        // userid was selected
        public void setUser(object sender, EventArgs e)
        {
            if (nameList.SelectedIndex > -1) // Checks if one was selected
            {
                currentUser.UserID = nameList.SelectedValue.ToString();
                currentUser.Username = nameBox.Text.ToString();
                MessageBox.Show("Logged in as: " + currentUser.Username);
            }
        }


        // Sets the list of names
        public void queryNameHelper(NpgsqlDataReader R)
        {
            nameList.Items.Add(R.GetString(0));
        }

        // Required for clicks
        protected virtual void OnClick()
        {
            EventHandler<EventArgs> handler = Click;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        // Required for box selection
        protected virtual void OnSelectedValueChanged()
        {
            EventHandler<EventArgs> handler = SelectedValueChanged;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

        // Puts all of the stuff where it belongs.
        public void createUI()
        {
            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);

            layout.BeginHorizontal();

            layout.BeginVertical();
            layout.BeginGroup("Set Current User", new Padding(10, 10, 10, 10));
            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Search User Name" });
            layout.BeginHorizontal();
            layout.AddAutoSized(nameBox);
            layout.AddAutoSized(search);
            layout.EndHorizontal();
            layout.EndVertical();
            layout.EndHorizontal();
            layout.BeginHorizontal();
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "User IDs" });
            layout.AddAutoSized(nameList);
            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndGroup();
            layout.EndVertical();

            layout.BeginHorizontal();
            layout.BeginGroup("Friends", new Padding(10, 10, 10, 10));
            layout.AddAutoSized(friendsGrid);
            layout.EndGroup();
            layout.EndHorizontal();

            layout.EndHorizontal();
        }

    }
}

