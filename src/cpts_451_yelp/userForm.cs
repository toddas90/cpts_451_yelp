using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    // Class for the user details window.
    public partial class userForm : Form
    {

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

        SharedInfo s = new SharedInfo(); // Shared info

        private UserInfo currentUser = new UserInfo();


        // Main entry point for user window.
        public userForm(UserInfo inUser) // Main Form
        {
            Title = "User Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution

            createUI(); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout

            currentUser = inUser;

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

        // Puts all of the stuff where it belongs.
        public void createUI()
        {
            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);

            layout.BeginHorizontal();

            layout.BeginHorizontal();
            layout.BeginGroup("Friends", new Padding(10, 10, 10, 10));
            layout.AddAutoSized(friendsGrid);
            layout.EndGroup();
            layout.EndHorizontal();

            layout.EndHorizontal();
        }

    }
}

