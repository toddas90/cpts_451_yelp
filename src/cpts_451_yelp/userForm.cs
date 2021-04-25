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
        DataStoreCollection<TipInfo> latestTipsData = new DataStoreCollection<TipInfo>();

        // Grid for friends of the user
        GridView friendsGrid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        GridView latestTips = new GridView<TipInfo>
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
            Title = "Friends"; // Title of Application
            MinimumSize = new Size(1700, 720); // Default resolution

            addColFriendGrid();
            addColTipGrid();
            createUI(); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout

            currentUser = inUser;
            queryFriends();
            queryTips();

        }

        public void queryFriends()
        {
            friendData.Clear();

            string cmd = @"SELECT friendid, username, totalLikes, averageStars, yelpingSince 
                        FROM FriendsWith, Users WHERE FriendsWith.friendid = Users.UserID AND FriendsWith.UserId = '" + currentUser.UserID + "' ;";
            s.executeQuery(cmd, queryFriendInfoHelper, true);
            friendsGrid.DataStore = friendData;
        }

        public void queryTips()
        {
            latestTipsData.Clear();

            string cmd = @"SELECT friendid, recentDate, username, textwritten, businessname, businesscity
                        FROM FriendsWith, Users, BusinessAddress, Business, Tip, (SELECT userid, MAX(datewritten) as recentDate FROM Tip GROUP BY userid) as Recent 
                        WHERE FriendsWith.friendid = Users.UserID AND FriendsWith.UserId = '" + currentUser.UserID + @"'
                        AND BusinessAddress.businessid = Business.businessid AND Tip.businessid = business.businessid AND Tip.datewritten = Recent.recentDate 
                        AND Tip.userid = Recent.userid AND Recent.userid = Friendswith.friendid ORDER BY recentDate DESC;";
            s.executeQuery(cmd, queryLatestTipsHelper, true);
            latestTips.DataStore = latestTipsData;
        }

        private void queryFriendInfoHelper(NpgsqlDataReader R)
        {
            friendData.Add(new UserInfo() // Made a business class to keep the info together
            {
                UserID = R.GetString(0),
                Username = R.GetString(1), // name
                likes = R.GetInt32(2), // total likes
                avgStars = Math.Round(R.GetDouble(3), 2), //average stars
                date = R.GetDateTime(4) // yelping since date
            });
        }

        private void queryLatestTipsHelper(NpgsqlDataReader R)
        {
            latestTipsData.Add(new TipInfo() // Made a business class to keep the info together
            {
                uid = R.GetString(0), // user id
                date = (DateTime)R.GetTimeStamp(1), // name
                name = R.GetString(2), // tip leaver's name
                text = R.GetString(3), // text in the tip
                businessname = R.GetString(4), // name of business
                city = R.GetString(5) //city
            });
        }
        private void addColTipGrid()
        {
            latestTips.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("name"),
                HeaderText = "Name",
                //Width = 300,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            latestTips.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("businessname"),
                HeaderText = "Business",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            latestTips.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("city"),
                HeaderText = "City",
                //Width = 120,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            latestTips.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("text"),
                HeaderText = "Text",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            latestTips.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("date"),
                HeaderText = "Date",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
        }

        private void addColFriendGrid()
        {
            friendsGrid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Username"),
                HeaderText = "Name",
                //Width = 300,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            friendsGrid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("likes"),
                HeaderText = "Total Likes",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            friendsGrid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("avgStars"),
                HeaderText = "Avg Stars",
                //Width = 120,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
            friendsGrid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("date"),
                HeaderText = "Yelping Since",
                //Width = 60,
                AutoSize = true,
                Resizable = false,
                Sortable = true,
                Editable = false
            });
        }
        // Puts all of the stuff where it belongs.
        public void createUI()
        {
            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);
            friendsGrid.Size = new Size(450, 650);
            latestTips.Size = new Size(1200, 650);

            layout.BeginHorizontal();

            layout.BeginHorizontal();
            layout.BeginGroup("Friends", new Padding(10, 10, 10, 10));
            layout.AddAutoSized(friendsGrid);
            layout.EndGroup();
            layout.BeginGroup("Latest Tips", new Padding(10, 10, 10, 10));
            layout.AddAutoSized(latestTips);
            layout.EndGroup();
            layout.EndHorizontal();

            layout.EndHorizontal();
        }

    }
}

