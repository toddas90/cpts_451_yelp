using Eto.Forms;
using Eto.Drawing;
using System.IO;

namespace cpts_451_yelp
{
    // Class for the user details window.
    public partial class GraphForm : Form
    {
        DynamicLayout layout = new DynamicLayout(); // Layout for the page
        // Main entry point for graph window.
        public GraphForm()
        {
            MinimumSize = new Size(600, 400);
            createUI();
            this.Content = layout;
        }

        // creates the graph window
        public void createUI()
        {
            // generic data for testing
            double[] data = {1.0, 2.0, 3.0, 4.0, 5.0};

            // create the bar series
            var series = new OxyPlot.Series.BarSeries()
            {
                StrokeColor = OxyPlot.OxyColors.Black,
                FillColor = OxyPlot.OxyColors.Blue,
                StrokeThickness = 1
            };

            // add data to bar series
            for (int i = 0; i < 5; i++)
            {
                series.Items.Add(new OxyPlot.Series.BarItem(data[i], i));
            }

            // create a model and add the bar to it
            var model = new OxyPlot.PlotModel
            {
                Title = "Bar Graph",
                Background = OxyPlot.OxyColors.White
            };
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis());
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

