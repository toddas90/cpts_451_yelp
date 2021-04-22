using System;
using Eto.Forms;
using Eto.OxyPlot;

namespace cpts_451_yelp.Wpf
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var pf = Eto.Platform.Detect;
			pf.Add(typeof(Plot.IHandler), () => new Eto.OxyPlot.Wpf.PlotHandler());
			new Application(pf).Run(new MainForm());
		}
	}
}
