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
using System.ServiceProcess;
using System.Text;

using NLog;

namespace WindowsServiceSA1C
{
	static class Program
	{
		/// <summary>
		/// This method starts the service.
		/// </summary>
		static void Main(string[] args)
		{
			//Logger logger = LogManager.GetCurrentClassLogger();
			//logger.Info(args.Length);
			//foreach (var str in args) {				
			//	logger.Info(str);
			//}
			// To run more than one service you have to add them here
			ServiceBase.Run(new ServiceBase[] { new WindowsServiceSA1C() });
		}
	}
}
