using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    // Class for the business details window.
    public partial class userForm : Form
    {

        public event EventHandler<EventArgs> Click;
        public event EventHandler<EventArgs> SelectedValueChanged;

        DataStoreCollection<TipInfo> data = new DataStoreCollection<TipInfo>();

        GridView friendsGrid = new GridView<TipInfo>
        {
            AllowMultipleSelection = true,
            AllowEmptySelection = true
        };

        // Bunch of gross variables.
        DynamicLayout layout = new DynamicLayout();
        TextBox nameBox = new TextBox
        {
            PlaceholderText = "Name"
        };

        TextBox usernameBox = new TextBox();
        TextBox starsBox = new TextBox();
        TextBox dateBox = new TextBox();
        TextBox fansBox = new TextBox();
        TextBox funnyBox = new TextBox();
        TextBox coolBox = new TextBox();
        TextBox usefulBox = new TextBox();
        TextBox tipcountBox = new TextBox();
        TextBox totallikesBox = new TextBox();
        TextBox latitudeBox = new TextBox();
        TextBox longitudeBox = new TextBox();
        ListBox nameList = new ListBox
        {
            Size = new Size(150, 100)
        };

        Button search = new Button
        {
            Text = "Search"
        };
        SharedInfo s = new SharedInfo();

        public UserInfo currentUser = new UserInfo();

        // Main entry point for business window.
        public userForm() // Main Form
        {
            Title = "User Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution

            createUI(); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout

            search.Click += new EventHandler<EventArgs>(queryName);
            nameList.SelectedValueChanged += new EventHandler<EventArgs>(setUser);
        }

        public void queryName(object sender, EventArgs e)
        {
            nameList.Items.Clear();
            string cmd = @"SELECT Users.userid, username FROM Users INNER JOIN UserLocation ON Users.userid = UserLocation.userid 
                        INNER JOIN UserRating ON UserLocation.userid = UserRating.userid WHERE username = '" + nameSearch() + "'";
            s.executeQuery(cmd, queryNameHelper, true);
        }


        public String nameSearch()
        {
            return nameBox.Text.ToString();
        }


        public void setUser(object sender, EventArgs e)
        {
            if (nameList.SelectedIndex > -1)
            {
                currentUser.UserID = nameList.SelectedValue.ToString();
                currentUser.Username = nameBox.Text.ToString();
                MessageBox.Show("Logged in as: " + currentUser.Username);
            }
        }


        public void queryNameHelper(NpgsqlDataReader R)
        {
            nameList.Items.Add(R.GetString(0));
        }

        protected virtual void OnClick()
        {
            EventHandler<EventArgs> handler = Click;
            if (null != Handler) handler(this, EventArgs.Empty);
        }

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

            layout.BeginVertical();
            layout.BeginGroup("User Information", new Padding(10, 10, 10, 10));
            layout.BeginVertical(padding: new Padding(0, 0, 0, 10));
            layout.AddAutoSized(new Label { Text = "Name: " });
            layout.AddRow(usernameBox);
            layout.AddAutoSized(new Label { Text = "Stars: " });
            layout.AddRow(starsBox);
            layout.AddAutoSized(new Label { Text = "Fans: " });
            layout.AddRow(fansBox);
            layout.AddAutoSized(new Label { Text = "Yelping Since: " });
            layout.AddRow(dateBox);
            layout.AddAutoSized(new Label { Text = "Funny: " });
            layout.AddAutoSized(new Label { Text = "Cool: " });
            layout.AddAutoSized(new Label { Text = "Useful: " });
            layout.BeginHorizontal();
            layout.AddRow(funnyBox);
            layout.AddRow(coolBox);
            layout.AddRow(usefulBox);
            layout.EndHorizontal();
            layout.EndVertical();
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

