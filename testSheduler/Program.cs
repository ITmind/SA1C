/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 06.05.2011
 * Время: 16:43
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using Quartz;
using Quartz.Impl;

namespace testSheduler
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			// TODO: Implement Functionality Here
			// construct a scheduler factory
			ISchedulerFactory schedFact = new StdSchedulerFactory();

			// get a scheduler
			IScheduler sched = schedFact.GetScheduler();
			sched.Start();

			// construct job info
			JobDetail jobDetail = new JobDetail("myJob", null, typeof(HelloJob));
			// fire every hour
			//Trigger trigger = TriggerUtils.MakeSecondlyTrigger(10);
			Trigger trigger = new CronTrigger("my","group1","0/10 * * * * ?");
			// start on the next even hour
			trigger.StartTimeUtc = TriggerUtils.GetEvenSecondDate(DateTime.UtcNow);
			trigger.Name = "myTrigger";
			sched.ScheduleJob(jobDetail, trigger);
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
			sched.Shutdown(false);
		}
	}
	
	public class HelloJob : IJob
	{
		
		//private static ILog _log = LogManager.GetLogger(typeof(HelloJob));
		
		/// <summary>
		/// Empty constructor for job initilization
		/// <p>
		/// Quartz requires a public empty constructor so that the
		/// scheduler can instantiate the class whenever it needs.
		/// </p>
		/// </summary>
		public HelloJob()
		{
		}
		
		/// <summary>
		/// Called by the <see cref="IScheduler" /> when a
		/// <see cref="Trigger" /> fires that is associated with
		/// the <see cref="IJob" />.
		/// </summary>
		public virtual void  Execute(JobExecutionContext context)
		{
			Console.WriteLine("10");
			// Say Hello to the World and display the date/time
			//_log.Info(string.Format("Hello World! - {0}", System.DateTime.Now.ToString("r")));
		}

	}

}