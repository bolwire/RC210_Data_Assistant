using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RC210_DataAssistant_V2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//Enable the annoying Beta message here
			//var betaForm = new BetaForm();
			//betaForm.ShowDialog();

			Application.Run(new Form1());
		}
	}
}
