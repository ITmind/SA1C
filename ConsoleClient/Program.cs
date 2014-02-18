using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using NLog;

using _1C;
using SA1CService;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
        	Logger logger = LogManager.GetCurrentClassLogger();
        	ServiceSA1C service = new ServiceSA1C();
        	service.LoadSettings();
        	var baseConfig = from conf in service.config.basesConfig
				where conf.Name == "ТестОбмена"
				select conf;

			if (baseConfig.Count() == 0)
			{
			}

			BaseConfig bc = baseConfig.First();
			
        	//var baseConfig = service.GetBaseConfig();
			string pathToPravila = Directory.GetCurrentDirectory()+@"\ПравилаОбменаДанными.xml";
        	
			using (_1Cv8 v8 = new _1Cv8(bc.baseInfo))
			{

				try
				{
					v8.Connect();
					v8.SaveUniversalXML("ОбменУправлениеТорговлейРозничнаяТорговля", "002",pathToPravila);
			
				}
				catch (Exception error)
				{
					string e = error.Message;
				}
			}
        }
    }
}
