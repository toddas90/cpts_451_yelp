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
            createUI();
        }

        private Random rand = new Random(0);
        private double[] RandomWalk(int points = 5, double start = 100, double mult = 50)
        {
            // return an array of difting random numbers
            double[] values = new double[points];
            values[0] = start;
            for (int i = 1; i < points; i++)
                values[i] = values[i - 1] + (rand.NextDouble() - .5) * mult;
            return values;
        }

        public void createUI()
        {
            layout.DefaultSpacing = new Size(5, 5);
            layout.Padding = new Padding(10, 10, 10, 10);

            // generate some random Y data
            int pointCount = 5;
            double[] ys1 = RandomWalk(pointCount);
            double[] ys2 = RandomWalk(pointCount);

            // create a series of bars and populate them with data
            var seriesA = new OxyPlot.Series.ColumnSeries()
            {
                Title = "Series A",
                StrokeColor = OxyPlot.OxyColors.Black,
                FillColor = OxyPlot.OxyColors.Red,
                StrokeThickness = 1
            };

            var seriesB = new OxyPlot.Series.ColumnSeries()
            {
                Title = "Series B",
                StrokeColor = OxyPlot.OxyColors.Black,
                FillColor = OxyPlot.OxyColors.Blue,
                StrokeThickness = 1
            };

            for (int i = 0; i < pointCount; i++)
            {
                seriesA.Items.Add(new OxyPlot.Series.ColumnItem(ys1[i], i));
                seriesB.Items.Add(new OxyPlot.Series.ColumnItem(ys2[i], i));
            }

            // create a model and add the bars into it
            var model = new OxyPlot.PlotModel
            {
                Title = "Bar Graph (Column Series)"
            };
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis());
            model.Series.Add(seriesA);
            model.Series.Add(seriesB);

            // load the model into the user control
            Plot p = new Plot();
            p.Model = model;

            layout.BeginHorizontal();
            layout.BeginVertical();
            layout.Add(p);
            layout.EndHorizontal();
            layout.EndVertical();
            //Eto.OxyPlot.Plot.IHandler.Model.set(model);
            //plotView1.Model = model;
        }

    }
}

