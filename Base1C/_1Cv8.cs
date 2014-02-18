using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Configuration;

namespace _1C
{
    public class _1Cv8 : Base1C
    {
        #region Function

        public _1Cv8()
            : base()
        {
        }

        public _1Cv8(BaseInfo settings):base(settings)
        {
            
        }

        public dynamic Object1C
        {
            get { return object1c; }
        }

        /// <summary>
        /// Подключение к 1с
        /// </summary>
		public override void Connect()
		{
		   
				if (object1c == null)
				{
					Type COM = null;
					if (baseInfo.TypeConnection == EnumTypeConnection.COM)
					{
						COM = Type.GetTypeFromProgID(baseInfo.Version1C.ToString() + ".ComConnector", true);
					}
					else if (baseInfo.TypeConnection == EnumTypeConnection.OLE)
					{
                        COM = Type.GetTypeFromProgID(baseInfo.Version1C.ToString() + ".Application", true);
					}
	
					connector = Activator.CreateInstance(COM);
	
					StringBuilder ConnectionString = new StringBuilder();
					ConnectionString.Append(baseInfo.ConnectionString);
                    //ConnectionString.Append(@";");
					ConnectionString.Append(@"Usr=" + (baseInfo.User == null ? @";" : @"""" + baseInfo.User + @""";"));
					ConnectionString.Append(@"Pwd=" + (baseInfo.Password == null ? @";" : @"""" + baseInfo.GetPassword() + @""";"));
					
					object1c = connector.Connect(ConnectionString.ToString());
				}
	
		}

        /// <summary>
        /// Отключить всех пользователей в SQL варианте
        /// </summary>
        public void KillUsers()
        {
            dynamic server = connector.ConnectServer(baseInfo.ServerName);

            //аутентификация с прав. админа
            server.AddAuthentication(baseInfo.User, baseInfo.Password);

            //создать объект нужной базы
            dynamic InfBase = server.CreateInfoBaseInfo();
            InfBase.Name = baseInfo.BaseName;

            //получить соединение
            dynamic BasesConections = server.GetIBConnections(InfBase);

            //разорвать соединения клиентов
            foreach (dynamic con in BasesConections)
            {
                server.Disconnection(con);
            }
        }
        /// <summary>
        /// Обновление конфигурации БД
        /// </summary>
        public void UpdateDB()
        {
            StringBuilder arguments = new StringBuilder();
            arguments.Append(@"CONFIG ");
			arguments.Append(baseInfo.ConnectionStringConsole);
            arguments.Append(@" /UpdateDBCfg");
            
            Process p = new Process();
            p.StartInfo.FileName = baseInfo.PathToExe;
            p.StartInfo.Arguments = arguments.ToString();

            p.Start();
            p.WaitForExit();
        }
        /// <summary>
        /// Создание архива БД
        /// </summary>
        public void Arch(string patchToFileArch)
        {
            
            //TODO: вставить проверку на валидность пути
            //проверяем существование дириктории для архивов баз
            if (!Directory.Exists(patchToFileArch))
                Directory.CreateDirectory(patchToFileArch);
            
            StringBuilder arguments = new StringBuilder();
            arguments.Append(@"CONFIG ");
            arguments.Append(baseInfo.ConnectionStringConsole);
            arguments.Append(@" /DumpIB""");
            arguments.Append(patchToFileArch);

            Process p = new Process();
            p.StartInfo.FileName = baseInfo.PathToExe;
            p.StartInfo.Arguments = arguments.ToString();

            p.Start();
            p.WaitForExit();

        }
        /// <summary>
        /// Загрузка изменений в БД
        /// </summary>
        public void LoadChanges()
        {
            //dynamic XMLReader = object1c.NewObject("ЧтениеXML");
            string patchToFile = Path.GetTempPath() + @"\Message" + baseInfo.Name + ".xml";
            //XMLReader.ОткрытьФайл(patchToFile);
            //dynamic obmenPlans = object1c.ПланыОбмена;
            //dynamic messageRecord = obmenPlans.СоздатьЧтениеСообщения();
            //messageRecord.НачатьЧтение(XMLReader);
            //obmenPlans.ПрочитатьИзменения(messageRecord,10);
            //messageRecord.ЗакончитьЧтение();

            //для перехвата ошибок 1С используем рефлексию
            object loadXML = InvokeMethod(object1c, "NewObject", "ЧтениеXML");
            InvokeMethod(loadXML, "ОткрытьФайл", patchToFile);
            object obmenPlans = GetProperty(object1c, "ПланыОбмена");
            object messageRecord = InvokeMethod(obmenPlans, "СоздатьЧтениеСообщения", "");
            InvokeMethod(messageRecord, "НачатьЧтение", loadXML);
            InvokeMethod(obmenPlans, "ПрочитатьИзменения", messageRecord,10);
            InvokeMethod(messageRecord, "ЗакончитьЧтение", "");
        }

        /// <summary>
        /// Выгрузка изменений из БД
        /// </summary>
        public void SaveChanges(string nameOfPlan, string codeOfNode)
        {
            //dynamic XMLSaver = object1c.NewObject("ЗаписьXML");
            string patchToFile = Path.GetTempPath() + @"\Message" + baseInfo.Name + ".xml";
            //XMLSaver.OpenFile(patchToFile);
            //dynamic obmenPlans = object1c.ПланыОбмена;
            //dynamic plan = obmenPlans.Полный;
            //dynamic node = plan.НайтиПоКоду(codeOfNode);
            //dynamic messageRecord = obmenPlans.СоздатьЗаписьСообщения();
            //messageRecord.НачатьЗапись(XMLSaver, node);
            //obmenPlans.ЗаписатьИзменения(messageRecord,10);
            //messageRecord.ЗакончитьЗапись();
            
            //для перехвата ошибок 1С используем рефлексию
            object saveXML = InvokeMethod(object1c, "NewObject", "ЗаписьXML");
            InvokeMethod(saveXML, "ОткрытьФайл", patchToFile);
            object obmenPlans = GetProperty(object1c, "ПланыОбмена");           
            object plan = GetProperty(obmenPlans, nameOfPlan);
            
            //ищем по коду
            object node = InvokeMethod(plan, "НайтиПоКоду", codeOfNode);
            //если не нашли, то по наименованию
            //TODO: возможно здесь будет ошибка
            if (((bool)InvokeMethod(node, "Пустая", "")))
            {
                node = InvokeMethod(plan, "НайтиПоНаименованию", codeOfNode);
            }

            object messageRecord = InvokeMethod(obmenPlans, "СоздатьЗаписьСообщения", "");
            InvokeMethod(messageRecord, "НачатьЗапись", saveXML, node);
            InvokeMethod(obmenPlans, "ЗаписатьИзменения", messageRecord,10);
            InvokeMethod(messageRecord, "ЗакончитьЗапись", "");
        }

        /// <summary>
        /// Создание начального образа БД
        /// </summary>
        /// <param name="nameOfPlan"></param>
        /// <param name="codeOfNode"></param>
        /// <param name="connectionStringNewDB"></param>
        public void CreatePeripherialDB(string nameOfPlan, string codeOfNode, string connectionStringNewDB)
        {
            dynamic obmenPlans = object1c.ПланыОбмена;
            dynamic plan = obmenPlans.nameOfPlan;
            object node = plan.НайтиПоКоду(codeOfNode);
            object1c.ПланыОбмена.СоздатьНачальныйОбраз(node, connectionStringNewDB);
        }

        /// <summary>
        /// Запуск внешней обработки
        /// </summary>
        /// <param name="pathToExtDataProcessor">Путь к внешней обработке</param>
        public void ExecuteExtDataProcessor(string pathToExtDataProcessor)
        {
            dynamic extDataProcessor = object1c.ExternalDataProcessors.Create(pathToExtDataProcessor);
            extDataProcessor.ВыполнитьОбработку();
        }

        #endregion
    }
}
