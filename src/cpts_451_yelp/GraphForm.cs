using Eto.Forms;
using Eto.Drawing;
using System.IO;
using Npgsql;

namespace cpts_451_yelp
{
    // Class for the user details window.
    public partial class GraphForm : Form
    {

        SharedInfo s = new SharedInfo();
        DynamicLayout layout = new DynamicLayout(); // Layout for the page

        // number of checkins per month, indexed by month-1 (Jan = 0, Feb=1, Mar=2, etc.)
        int[] checkinCounts = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

        // Main entry point for graph window.
        public GraphForm(string bid)
        {
            MinimumSize = new Size(600, 400);
            this.loadCheckinData(bid);
            createUI();
            this.Content = layout;
        }

        // gets checkin data from the database
        private void loadCheckinData(string bid)
        {
            string cmd = @"SELECT EXTRACT(month from checkindate) as month, count(*) as count 
                           FROM checksin 
                           WHERE businessid = '" + bid + @"' 
                           GROUP BY EXTRACT(MONTH from checkindate)";
            s.executeQuery(cmd, checkinQueryResult, true);
        }

        private void checkinQueryResult(NpgsqlDataReader R)
        {
            // no query data, this means the business has no checkins
            if (!R.IsOnRow)
                return;
            
            // set checkin count values using query result
            do
            {
                double month = R.GetDouble(0);
                int count = R.GetInt32(1);

                this.checkinCounts[(int)month - 1] = count;
            } while (R.Read());
        }

        // creates the graph window
        public void createUI()
        {
            // create the bar series
            var series = new OxyPlot.Series.BarSeries
            {
                StrokeColor = OxyPlot.OxyColors.Black,
                FillColor = OxyPlot.OxyColors.Orange,
                StrokeThickness = 1,
                LabelPlacement = OxyPlot.Series.LabelPlacement.Inside,
                LabelFormatString = "{0:}"
            };

            // add data to bar series
            for (int i = 0; i < 12; i++)
            {
                series.Items.Add(new OxyPlot.Series.BarItem {Value = this.checkinCounts[i]});
            }

            // create a model and add the bar to it
            var model = new OxyPlot.PlotModel
            {
                Title = "Checkins Per Month",
                Background = OxyPlot.OxyColors.White
            };

            // add axis label
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                ItemsSource = new[]
                {
                    "January",
                    "February",
                    "March",
                    "April",
                    "May",
                    "June",
                    "July",
                    "August",
                    "September",
                    "October",
                    "November",
                    "December"
                }
            });
            model.Series.Add(series);

            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);

            const int plotWidth = 600;
            const int plotHeight = 400;

            Bitmap bmap;

            var pf = Eto.Platform.Detect;

            // convert the oxyplot model to a bitmap base on platform
            if (pf.IsWpf)
            {
                var pngStream = new MemoryStream();

                var exporter = new OxyPlot.ImageSharp.PngExporter(plotWidth, plotHeight);
                exporter.Export(model, pngStream);

                bmap = new Bitmap(pngStream);
            }
            else {
                OxyPlot.ImageSharp.PngExporter.Export(model, "temp.png", plotWidth, plotHeight);

                bmap = new Bitmap("temp.png");

                if (File.Exists("temp.png"))
                {
                    File.Delete("temp.png");
                }
            }

            layout.BeginHorizontal();
            layout.BeginVertical();
            layout.Add(bmap);
            layout.EndHorizontal();
            layout.EndVertical();
        }

    }
}

