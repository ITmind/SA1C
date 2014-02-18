/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 12.04.2011
 * Время: 16:01
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using NLog;
using SA1CService;

namespace WindowsServiceSA1C
{
	public class WindowsServiceSA1C : ServiceBase
	{
		public const string MyServiceName = "WindowsServiceSA1C";
		ServiceSA1C _service;
		//Thread th;
		
		public WindowsServiceSA1C()
		{
			InitializeComponent();
			_service = new ServiceSA1C();
		}
		
		private void InitializeComponent()
		{
			this.ServiceName = MyServiceName;
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// Start this service.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
//			logger.Info(args.Length);
//			foreach (var str in args) {				
//				logger.Info(str);
//			}
			
			string[] imagePathArgs = Environment.GetCommandLineArgs();
			foreach (var str in imagePathArgs) {				
				logger.Debug(str);
			}
			
			Directory.SetCurrentDirectory(imagePathArgs[1]);
			
			//th = new Thread(new ThreadStart(StartService));
			//th.Start();
			_service.LoadSettings();
			string result = _service.StartService();
			_service.StartSheduler();
			logger.Info(result);
		}
		
		private void StartService()
		{
			Logger logger = LogManager.GetCurrentClassLogger();			
			//logger.Info("New thread");
			_service.LoadSettings();
			string result = _service.StartService();
			_service.StartSheduler();
			logger.Info(result);
		}
		
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			//th.Abort();
			_service.StopService();
			Thread.Sleep(1000); //подождем секунду, что бы все отключилось
			AddLog("Stop");
		}
		
		public void AddLog(string log)
		{
			try
			{
				if (!EventLog.SourceExists("WindowsServiceSA1C"))
				{
					EventLog.CreateEventSource("WindowsServiceSA1C", "WindowsServiceSA1C");
				}
				
				EventLog myLog = new EventLog();
				myLog.Source = "WindowsServiceSA1C";
				myLog.WriteEntry(log);
			}
			catch{}
		}
	}
}
