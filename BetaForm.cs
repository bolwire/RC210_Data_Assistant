using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RC210_DataAssistant_V2
{
	public partial class BetaForm : Form
	{
		public BetaForm()
		{
			InitializeComponent();
		}

		private void buttonSupportGroup_Click(object sender, EventArgs e)
		{
			Process.Start("https://groups.google.com/forum/#!forum/rc210_data_assistant");
		}

		private void BetaForm_Load(object sender, EventArgs e)
		{
			buttonOK.Text = @"30";
			timer1.Enabled = true;

		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			buttonOK.Text = (Convert.ToInt32(buttonOK.Text) - 1).ToString(CultureInfo.InvariantCulture);

			if (buttonOK.Text == @"0")
			{
				buttonOK.Text = @"OK";
				buttonOK.Enabled = true;
				timer1.Enabled = false;
			}
		}

		private void buttonAbout_Click(object sender, EventArgs e)
		{
			var formAbout = new FormAbout();
			formAbout.ShowDialog();
		}

		private void label4_DoubleClick(object sender, EventArgs e)
		{
			buttonOK.Text = @"OK";
			buttonOK.Enabled = true;
			timer1.Enabled = false;
		}
	}
}
