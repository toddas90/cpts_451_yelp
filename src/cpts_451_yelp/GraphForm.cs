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

        public void createUI()
        {
            // generic data for testing
            double[] data = {1.0, 2.0, 3.0, 4.0, 5.0};

            // create the bar series
            var series = new OxyPlot.Series.ColumnSeries()
            {
                Title = "Data",
                StrokeColor = OxyPlot.OxyColors.Black,
                FillColor = OxyPlot.OxyColors.Blue,
                StrokeThickness = 1
            };

            // add data to bar series
            for (int i = 0; i < 5; i++)
            {
                series.Items.Add(new OxyPlot.Series.ColumnItem(data[i], i));
            }

            // create a model and add the bar to it
            var model = new OxyPlot.PlotModel
            {
                Title = "Bar Graph"
            };
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis());
            model.Series.Add(series);

            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);

            var pf = Eto.Platform.Detect;

            if (pf.IsWpf) {

                var pngStream = new MemoryStream();

                // export the model to a png format
                var pngExporter = new OxyPlot.Wpf.PngExporter { Width = 600, Height = 400, Background = OxyPlot.OxyColors.White };
                pngExporter.Export(model, pngStream);

                // create bitmap from png data so eto can display it
                Bitmap b = new Bitmap(pngStream);

                layout.BeginHorizontal();
                layout.BeginVertical();
                layout.Add(b);
                layout.EndHorizontal();
                layout.EndVertical();
            }
        }

    }
}

