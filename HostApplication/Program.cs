using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using NLog;

namespace HostApplication
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
			try{
				Application.Run(new Form1());
			}
			catch(Exception e){
				Logger logger = LogManager.GetCurrentClassLogger();
				logger.Error(e.Message);
				if(e.InnerException != null){
					logger.Error(e.InnerException.Message);
				}
				throw e;
			}
		}
	}
}
