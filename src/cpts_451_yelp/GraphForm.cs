using System;
using Eto.OxyPlot;
using Eto.Forms;
using Eto.Drawing;

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
            // generate some random Y data
            double[] data = {1.0, 2.0, 3.0, 4.0, 5.0};

            // create a series of bars and populate them with data
            var series = new OxyPlot.Series.ColumnSeries()
            {
                Title = "Data",
                StrokeColor = OxyPlot.OxyColors.Black,
                FillColor = OxyPlot.OxyColors.Blue,
                StrokeThickness = 1
            };

            for (int i = 0; i < 5; i++)
            {
                series.Items.Add(new OxyPlot.Series.ColumnItem(data[i], i));
            }

            // create a model and add the bars into it
            var model = new OxyPlot.PlotModel
            {
                Title = "Bar Graph"
            };
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis());
            model.Series.Add(series);

            // load the model into the user control
            Plot p = new Plot();
            p.Model = model;

            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);

            layout.BeginHorizontal();
            layout.BeginVertical();
            layout.Add(p);
            layout.EndHorizontal();
            layout.EndVertical();
        }

    }
}

