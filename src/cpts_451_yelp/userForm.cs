using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    // Class for the business details window.
    public partial class userForm : Form
    {
        // Bunch of gross variables.
        DynamicLayout layout = new DynamicLayout();
        TextBox nameBox = new TextBox();

        ListBox nameList = new ListBox
        {
            Size = new Size(150, 100)
        };
        SharedInfo s = new SharedInfo();

        // Main entry point for business window.
        public userForm(string user) // Main Form
        {
            Title = "User Details"; // Title of Application
            MinimumSize = new Size(600, 400); // Default resolution

            createUI(); // Puts everything where it belongs
            this.Content = layout; // Instantiates the layout
        }

        // Puts all of the stuff where it belongs.
        public void createUI()
        {
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);
        }

    }
}

