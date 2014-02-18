/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 15.03.2011
 * Время: 13:50
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;

using CsHTTPServer;
using NLog;
using Quartz;
using Quartz.Impl;

namespace SA1CService
{
	/// <summary>
	/// Description of ServiceSA1C_Service.
	/// </summary>
	public partial class ServiceSA1C : IService1C, IFileTransfer,IWebServ
	{
		ServiceHost host;
		IScheduler sched;
		MyServer HTTPServer = null;
		ServiceHost webHost = null;
		
		public string StartService(){
			Logger logger = LogManager.GetCurrentClassLogger();
			string result="";
			
			try{
				Uri baseAddress = new Uri(config.HostSetting.BaseAdress);
				logger.Info(config.HostSetting.BaseAdress);
				host = new ServiceHost(typeof(ServiceSA1C),baseAddress);
				
				//если нет декларативных настроек, то пропишем вручную
				//пропишем МЕХ
				if(host.Description.Behaviors.Find<ServiceMetadataBehavior>()==null){
					ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
//					smb.HttpGetEnabled = true;
//					smb.HttpGetUrl = baseAddress;
//					smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
					host.Description.Behaviors.Add(smb);
				}
				
				if(host.Description.Endpoints.Find(typeof(SA1CService.IService1C))==null){
					NetTcpBinding TCPbinding = new NetTcpBinding(SecurityMode.None);
					TCPbinding.CloseTimeout = TimeSpan.FromHours(0.5);
					TCPbinding.OpenTimeout = TimeSpan.FromHours(0.5);
					TCPbinding.SendTimeout = TimeSpan.FromHours(13);
					TCPbinding.TransferMode = TransferMode.Buffered;
					TCPbinding.MaxReceivedMessageSize = 429496729;
					host.AddServiceEndpoint("SA1CService.IService1C",TCPbinding,config.HostSetting.BaseAdress+"Service1C");
				}
				
				if(host.Description.Endpoints.Find(typeof(SA1CService.IFileTransfer))==null){
					NetTcpBinding streamBinding = new NetTcpBinding(SecurityMode.None);
					streamBinding.CloseTimeout = TimeSpan.FromHours(0.5);
					streamBinding.OpenTimeout = TimeSpan.FromHours(0.5);
					streamBinding.SendTimeout = TimeSpan.FromHours(13);
					streamBinding.TransferMode = TransferMode.Buffered;
					streamBinding.MaxReceivedMessageSize = 429496729;
					host.AddServiceEndpoint("SA1CService.IFileTransfer",streamBinding,config.HostSetting.BaseAdress+"FileTransfer");
				}
				
				if(host.Description.Endpoints.Find(typeof(IMetadataExchange))==null){
					BindingElement bindingElement = new TcpTransportBindingElement();
					CustomBinding binding = new CustomBinding(bindingElement);
					host.AddServiceEndpoint(typeof(IMetadataExchange),binding,"MEX");
				}
				
				host.Faulted += delegate(object sender, EventArgs e) {
					logger.Error("Host Faulted");
					logger.Error(e.ToString);
				};
				
				host.UnknownMessageReceived += delegate(object sender, UnknownMessageReceivedEventArgs e) {
					logger.Error("Host UnknownMessageReceived");
					logger.Error(e.Message);
					
				};
				
				host.Open();
				
				//и web сервис
				if(config.HostSetting.PortWebService !=0 ){
					webHost = new WebServiceHost(typeof(ServiceSA1C));
					var webBinding = new WebHttpBinding();
					webBinding.CrossDomainScriptAccessEnabled = true;
					webHost.AddServiceEndpoint(typeof(IWebServ), webBinding, config.HostSetting.BaseAdressHttp+"WebServ");
					webHost.Open();
				}
				
				//http server
				if(config.HostSetting.PortHTTPServer !=0){
					HTTPServer = new MyServer(config.HostSetting.PortHTTPServer,".");
					HTTPServer.Start();
				}

				result = "Служба запущена... ";
			}
			catch(Exception e){
				logger.Error(e.Message);
				result = e.Message;
			}
			
			return result;
		}
		
		public void StopService(){
			Logger logger = LogManager.GetCurrentClassLogger();
			if (host != null){
				host.Abort();
				logger.Debug("Stop service");
			}
			if(sched!=null){
				sched.Shutdown();
				logger.Debug("Stop sheduler");
			}
			if(webHost!=null){
				webHost.Abort();
				logger.Debug("Stop http server");
			}
			if(HTTPServer!=null){
				HTTPServer.Stop();
				logger.Debug("Stop http server");
			}
		}
		
		public void StartSheduler(){
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug("Configure sheduler");
			
			ISchedulerFactory schedFact = new StdSchedulerFactory();

			sched = schedFact.GetScheduler();

			var baseConfig = from conf in config.basesConfig
				where conf.JobSheduler.isEnable == true
				select new { conf.Name, conf.JobSheduler.Expression };

			if (baseConfig.Count() != 0)
			{
				foreach(var c in baseConfig){
					JobDetail jobDetail = new JobDetail(c.Name, null, typeof(ExchangeJob));
					Trigger trigger = new CronTrigger(c.Name,null,c.Expression);
					// со следующей секунды
					trigger.StartTimeUtc = TriggerUtils.GetEvenSecondDate(DateTime.UtcNow);
					//trigger.Name = "myTrigger";
					sched.ScheduleJob(jobDetail, trigger);
				}
			}
			
			// и будем проверять изменение расписания каждые 30 минут
			JobDetail jobDetail2 = new JobDetail("Setting", null, typeof(SettingJob));
			Trigger trigger2 = TriggerUtils.MakeMinutelyTrigger(30); //TODO
			trigger2.Name = "Setting";
			// со следующей секунды
			trigger2.StartTimeUtc = TriggerUtils.GetEvenSecondDate(DateTime.UtcNow);
			sched.ScheduleJob(jobDetail2, trigger2);
			
			logger.Debug("Start sheduler");
			sched.Start();
		}
		
	}
	
	public class ExchangeJob : IStatefulJob
	{
		public ExchangeJob()
		{
		}
		
		public virtual void  Execute(JobExecutionContext context)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			string instName = context.JobDetail.Name;
			
			var jobs = context.Scheduler.GetCurrentlyExecutingJobs();
			foreach (JobExecutionContext job in jobs) {
				if (job.Trigger==context.Trigger && !(job.JobInstance==this)) {
					logger.Debug("There's another instance running, so leaving" + this);
					return;
				}

			}
			
			logger.Debug("Start job: "+instName);
			
			//запустим обмен в отдельном экземпляре
			ServiceSA1C _serviceSA1C = new ServiceSA1C();
			_serviceSA1C.ExecuteExchange(context.JobDetail.Name, false);
		}

	}
	
	public class SettingJob : IJob
	{
		public SettingJob()
		{
		}
		
		public virtual void  Execute(JobExecutionContext context)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			string instName = context.JobDetail.Name;
			
			var jobs = context.Scheduler.GetCurrentlyExecutingJobs();
			foreach (JobExecutionContext job in jobs) {
				if (job.Trigger==context.Trigger && !(job.JobInstance==this)) {
					logger.Debug("There's another instance running, so leaving" + this);
					return;
				}

			}
			
			//logger.Debug("Start job: "+instName);
			ServiceSA1C _serviceSA1C = new ServiceSA1C();
			
			var baseConfig = from conf in _serviceSA1C.config.basesConfig
				select new { conf.Name, conf.JobSheduler.Expression,conf.JobSheduler.isEnable };

			foreach(var c in baseConfig){
				//logger.Debug("Set : "+c.Name);
				Trigger trigger = context.Scheduler.GetTrigger(c.Name,null);
				if(!c.isEnable && trigger != null){
					//надо отключить
					logger.Debug("Delete job");
					context.Scheduler.DeleteJob(c.Name,null);
					continue;
				}
				
				if((c.isEnable && trigger == null)){
					//нужно создать
					logger.Debug("Create new job");
					JobDetail jobDetail = new JobDetail(c.Name, null, typeof(ExchangeJob));
					trigger = new CronTrigger(c.Name,null,c.Expression);
					// со следующей секунды
					trigger.StartTimeUtc = TriggerUtils.GetEvenSecondDate(DateTime.UtcNow);
					//trigger.Name = "myTrigger";
					context.Scheduler.ScheduleJob(jobDetail, trigger);
					continue;
				}
				
				if(trigger!=null){
					//Trigger newTrigger = new CronTrigger(c.Name,null,c.Expression);
					CronTrigger tg = (CronTrigger)(trigger);
					
					//int result = trigger.CompareTo(newTrigger);
					if(tg.CronExpressionString != c.Expression){
						logger.Debug("Trigger change. Delete old and create new");
						context.Scheduler.DeleteJob(c.Name,null);
						JobDetail jobDetail = new JobDetail(c.Name, null, typeof(ExchangeJob));
						trigger = new CronTrigger(c.Name,null,c.Expression);
						trigger.StartTimeUtc = TriggerUtils.GetEvenSecondDate(DateTime.UtcNow);
						context.Scheduler.ScheduleJob(jobDetail, trigger);
					}
				}
				// со следующей секунды
				//trigger.StartTimeUtc = TriggerUtils.GetEvenSecondDate(DateTime.UtcNow);
				//trigger.Name = "myTrigger";
				//sched.ScheduleJob(jobDetail, trigger);
			}
			
			
		}

	}
	
}
