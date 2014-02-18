using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using NLog;
using SA1CService;

namespace HostApplication
{
	public partial class Form1 : Form
	{
		Thread th;
		ServiceSA1C _service;

		public Form1()
		{
			InitializeComponent();
			_service = new ServiceSA1C();
		}

		private void выходToolStripMenuItem_Click(object sender, EventArgs e)
		{
			th.Abort();
			_service.StopService();
			Application.Exit();
			
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Hide();
			}

		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			this.Hide();
		}

		private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			th = new Thread(new ThreadStart(StartService));
			th.IsBackground = true;
			th.Start();			
		}

		private void StartService()
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			Func<string, int> AddStr = str => report.Items.Add(str);
			
			string result = _service.StartService();
			_service.StartSheduler();
			report.Invoke(AddStr, result);	
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			th.Abort();
			_service.StopService();
		}
		


	}
}
