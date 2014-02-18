using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using NLog;

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

		public void Disconnect(){
			// Clean up unmanaged resources
			if (object1c != null)
			{
				Marshal.ReleaseComObject(object1c);
				Marshal.ReleaseComObject(connector);
			}

			object1c  = null;
			connector = null;
		}
		
		/// <summary>
		/// Отключить всех пользователей в SQL варианте
		/// </summary>
		public void KillUsers()
		{
			if(baseInfo.IsFileDB){
				Logger logger = LogManager.GetCurrentClassLogger();
				logger.Error("Для файловой БД нельзя сделать разрыв соединений пользователя");
				return;
			}
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
		/// Запустить 1С с параметром /С
		/// </summary>
		/// <param name="Mode"></param>
		/// <param name="Prarm"></param>
		/// <param name="BlockPassword"></param>
		public void Start1C(string mode, string param = "",string blockPassword = ""){
			Logger logger = LogManager.GetCurrentClassLogger();
			
			StringBuilder arguments = new StringBuilder();
			arguments.Append(mode);
			arguments.Append(baseInfo.ConnectionStringConsole);
			if(param != String.Empty){
				arguments.Append(@" /C"+param);
			}

			if(blockPassword != String.Empty){
				arguments.Append(@" /UC"+blockPassword);
			}
			
			arguments.Append(@" /DisableStartupMessages");
			
			Process p = new Process();
			if(baseInfo.PathToExe != null && baseInfo.PathToExe != String.Empty ){
				p.StartInfo.FileName = baseInfo.PathToExe;
			}
			else{
				p.StartInfo.FileName = Helper1C.GetPathExe(baseInfo.Version1C);
				
			}
			p.StartInfo.Arguments = arguments.ToString();

			try{
				p.Start();
				p.WaitForExit();
			}
			catch(Win32Exception e){
				//TODO: Убрать вывод в лог
				logger.Error("Ошибка запуска 1С "+e.Message);
			}
			
			Thread.Sleep(1000); //Точно дождемся :)
		}
		
		/// <summary>
		/// Обновление конфигурации БД
		/// </summary>
		/// <param name="dinamyc">обновлять динамически</param>
		public string UpdateDB(bool dinamyc, TypeKillUsers typeKill = TypeKillUsers.Kill, string blockPassword = "")
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			string isUpdate = "ok";
			bool isBlock = false;
			
			//выкинем пользователей
			if(!dinamyc){
				if(UserPresent()){
					DisableUser(typeKill);
					//Thread.Sleep(1000);
					//подождем выхода пользователей
					
					if(typeKill == TypeKillUsers.UserProc){
						logger.Debug("Ждем 1.5 минуты");
						//подождем 1,5 минуты пока пользователи отключатся.
						Thread.Sleep(90000);
					}
					else if(typeKill == TypeKillUsers.StandartParam){
						//подождем 10 секунд
						Thread.Sleep(9000);
					}
					else if(typeKill == TypeKillUsers.None){
						//подождем 1 секунд
						//Thread.Sleep(1000);
					}
				}
				//если не получилось отключить пользователей то выходим
				if(UserPresent()){
					logger.Error("Ошибка выполнения обновления: в базе присутствуют пользователи");
					return "Ошибка выполнения обновления. В базе присутствуют пользователи. Необходимо закрыть все запущенные сеансы 1С и заново повторить обмен";
				}
			}
			
			StringBuilder arguments = new StringBuilder();
			arguments.Append(@"CONFIG ");
			arguments.Append(baseInfo.ConnectionStringConsole);
			arguments.Append(@" /UpdateDBCfg");// -force");
			//if (dinamyc){
			//динмачиское
			//arguments.Append(@" /UpdateDBCfg");// -force");
			//}
			//else{
			//	arguments.Append(@" /UpdateDBCfg -WarningsAsErrors");
			//}

			arguments.Append(@" /Out");
			arguments.Append(@"""");
			arguments.Append(Path.GetTempPath() + @"SA1C\log");
			arguments.Append(@"""");
			
			if(blockPassword != String.Empty){
				arguments.Append(@" /UC"+blockPassword);
			}
			
			Process p = new Process();
			if(baseInfo.PathToExe != null && baseInfo.PathToExe != String.Empty ){
				p.StartInfo.FileName = baseInfo.PathToExe;
			}
			else{
				p.StartInfo.FileName = Helper1C.GetPathExe(baseInfo.Version1C);
				
			}
			p.StartInfo.Arguments = arguments.ToString();

			try{
				p.Start();
				p.WaitForExit();
				isUpdate = "ok";
			}
			catch(Win32Exception e){
				logger.Error("Ошибка выполнения обновления "+e.Message+". "+p.StartInfo.FileName);
				isUpdate = "Ошибка выполнения обновления. "+e.Message+". "+p.StartInfo.FileName;
				return isUpdate;
			}
			
			Thread.Sleep(3000); // Почемуто сразу не завершается
			p.Dispose();
			
			if(!dinamyc){
				EnableUser(typeKill);
			}
			
			//проверим, были ли ошибки при обновлении
			//для этого посмотрим лог файл
			//проверим, есть ли доступ к файлу и если нет
			//ждем пока файл освободит другой процесс
			int numError = 0;
			while (true)
			{
				try
				{
					FileStream fs = File.Open(Path.GetTempPath() + @"SA1C\log", FileMode.Open, FileAccess.Read, FileShare.Read);
					fs.Close();
					break; //выходим из бесконечного цикла
				}
				catch
				{
					Thread.Sleep(1000);
					logger.Error("Файл"+Path.GetTempPath() + @"SA1C\log"+" заблокирован");
					numError++;
					if(numError>40){ //по попыток, 10-мало
						isUpdate = "Файл"+Path.GetTempPath() + @"SA1C\log"+" заблокирован";
						return isUpdate;
					}
					//TODO: ОШИБКА нельзя ждать вечно
				}
			}
			
			using(StreamReader log = File.OpenText(Path.GetTempPath() + @"SA1C\log")){
				string str = log.ReadLine();
				while(str != null){
					if(str.Contains("Ошибка")){
						
						logger.Error("Ошибка выполнения обновления: "+str+". Подробности в файле "+Path.GetTempPath() + @"SA1C\log");
						
						isUpdate = "Ошибка выполнения обновления: "+str+". Подробности в файле "+Path.GetTempPath() + @"SA1C\log";
					}
					
					if(str.Contains("блокировки")){
						isBlock = true;
					}
					
					str = log.ReadLine();
				}
			}
			
			//если не получилось обновить динамически то попробуем обновить не динамически
			if(isBlock && dinamyc){
				isUpdate = UpdateDB(false, typeKill, blockPassword);
			}
			
			return isUpdate;
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
		/// Определяет признак изменения конфиуграции
		/// </summary>
		/// <returns>конфигурация изменена</returns>
		public bool ConfigChanged(){
			object configChanged = InvokeMethod(object1c, "ConfigurationChanged", "");
			return (bool)configChanged;
		}
		
		/// <summary>
		/// Есть ли пользователи в базе
		/// </summary>
		/// <returns>Истина если есть</returns>
		public bool UserPresent(){
			//проверку сделаем через установку монопольного режима
			try{
				this.Connect();
				InvokeMethod(object1c, "SetExclusiveMode", "Истина");
				this.Dispose();
				return false;
			}
			catch{
				return true;
			}
		}
		/// <summary>
		/// Отключить пользователей
		/// </summary>
		public void DisableUser(TypeKillUsers typeKill){
			Logger logger = LogManager.GetCurrentClassLogger();
			switch (typeKill) {
				case TypeKillUsers.Kill:
					KillUsers();
					break;
				case TypeKillUsers.StandartParam:
					Start1C("ENTERPRISE","ЗавершитьРаботуПользователей","ПакетноеОбновлениеКонфигурацииИБ");
					break;
				case TypeKillUsers.UserProc:
					this.Connect();
					try{
						logger.Debug("Вызов процедуры ОтключитьПользователей");
						//InvokeMethod(object1c, "ОтключитьПользователей", "");
						object1c.ОтключитьПользователей();
					}
					catch(Exception e){
						if(e.InnerException!=null){
							logger.Error(e.InnerException.Message);
						}
						else{
							logger.Error(e.Message);
						}
					}
					this.Disconnect();
					break;
				case TypeKillUsers.None:
					break;
				default:
					throw new Exception("Invalid value for TypeKillUsers");
			}
			
		}
		
		/// <summary>
		/// Включить пользователей
		/// </summary>
		public void EnableUser(TypeKillUsers typeKill){
			switch (typeKill) {
				case TypeKillUsers.Kill:
					break;
				case TypeKillUsers.StandartParam:
					Start1C("ENTERPRISE","РазрешитьРаботуПользователей","ПакетноеОбновлениеКонфигурацииИБ");
					break;
				case TypeKillUsers.UserProc:
					this.Connect();
					try{
						InvokeMethod(object1c, "ВключитьПользователей", "");
					}
					catch{
						
					}
					this.Disconnect();
					break;
				case TypeKillUsers.None:
					break;
				default:
					throw new Exception("Invalid value for TypeKillUsers");
			}
		}
		
		public void ExchangeUniversalXML(int kod){
			object Catalog = GetProperty(object1c, "Справочники");
			object SettingsExchange = GetProperty(Catalog, "НастройкиВыполненияОбмена");
			object node = InvokeMethod(SettingsExchange, "НайтиПоКоду", kod);
			InvokeMethod(object1c, "ПроцедурыОбменаДанными.ВыполнитьОбменДаннымиПоПроизвольнойНастройке", node);
			
			Marshal.ReleaseComObject(Catalog); Catalog = null;
			Marshal.ReleaseComObject(SettingsExchange); SettingsExchange = null;
			Marshal.ReleaseComObject(node); node = null;
			
		}
		
		public void LoadUniversalXML(){
			string patchToFile = Path.GetTempPath() + @"SA1C\In\Message" + baseInfo.Name + ".xml";
			string patchToProtokol = Path.GetTempPath() + @"SA1C\In\Protokol_" + baseInfo.Name + ".txt";
			dynamic XMLExchange = object1c.Обработки.УниверсальныйОбменДаннымиXML.Создать();
			
			XMLExchange.ИмяФайлаОбмена = patchToFile;
			XMLExchange.ЗагружатьДанныеВРежимеОбмена = true;
			XMLExchange.ЗаписыватьВИнформационнуюБазуТолькоИзмененныеОбъекты = true;
			XMLExchange.ОбъектыПоСсылкеЗагружатьБезПометкиУдаления = true;
			XMLExchange.ОптимизированнаяЗаписьОбъектов = true;
			XMLExchange.ЗаписыватьРегистрыНаборамиЗаписей = true;
			XMLExchange.ИспользоватьТранзакции = true;
			XMLExchange.КоличествоОбъектовНаТранзакцию = 10;
			XMLExchange.ИмяФайлаПротоколаОбмена = patchToProtokol;
			XMLExchange.ЗапоминатьЗагруженныеОбъекты = true;
			XMLExchange.ОбработчикиСобытийЧитаемИзФайлаПравилОбмена = true;
			XMLExchange.РежимОбмена = "Загрузка";

			
			XMLExchange.ВыполнитьЗагрузку();
			if(XMLExchange.ФлагОшибки){
				throw new Exception("Ошибка загрузки универсального обмена XML. Для определения ошибки прочитайте протокол "+patchToProtokol);
			}
			Marshal.ReleaseComObject(XMLExchange); XMLExchange = null;
		}
		
		public void SaveUniversalXML(string nameOfPlan, string codeOfNode,string pathToPr){
			string patchToFile = Path.GetTempPath() + @"SA1C\Out\Message" + baseInfo.Name + ".xml";
			string patchToProtokol = Path.GetTempPath() + @"SA1C\Out\Protokol_" + baseInfo.Name + ".txt";
			object obmenPlans = GetProperty(object1c, "ПланыОбмена");
			object plan = GetProperty(obmenPlans, nameOfPlan);
			//ищем по коду
			object node = InvokeMethod(plan, "НайтиПоКоду", codeOfNode);
			if (((bool)InvokeMethod(node, "Пустая", "")))
			{
				node = InvokeMethod(plan, "НайтиПоНаименованию", codeOfNode);
			}
			
			dynamic XMLExchange = object1c.Обработки.УниверсальныйОбменДаннымиXML.Создать();
			
			XMLExchange.ИмяФайлаПравилОбмена = pathToPr;
			XMLExchange.ИмяФайлаОбмена = patchToFile;
			XMLExchange.ИспользоватьТранзакцииПриВыгрузкеДляПлановОбмена = true;
			XMLExchange.КоличествоЭлементовВТранзакцииПриВыгрузкеДляПлановОбмена = 10;
			XMLExchange.ТипУдаленияРегистрацииИзмененийДляУзловОбменаПослеВыгрузки = 1;
			XMLExchange.ИмяФайлаПротоколаОбмена = patchToProtokol;
			XMLExchange.ОбработчикиСобытийЧитаемИзФайлаПравилОбмена = true;
			XMLExchange.РежимОбмена = "Выгрузка";
			
			XMLExchange.ЗагрузитьПравилаОбмена();
			SetNodeToTree(XMLExchange.ТаблицаПравилВыгрузки.Строки,node);
			XMLExchange.ВыполнитьВыгрузку();
			if(XMLExchange.ФлагОшибки){
				throw new Exception("Ошибка выгрузки универсального обмена XML. Для определения ошибки прочитайте протокол "+patchToProtokol);
			}
			Marshal.ReleaseComObject(XMLExchange); XMLExchange = null;
		}
		
		public void SetNodeToTree(dynamic tree, dynamic node){
			foreach(dynamic str in tree){
				
				if(str.ЭтоГруппа){
					SetNodeToTree(str.Строки, node);
				}
				else{
					str.СсылкаНаУзелОбмена = node;
				}
			}
			
		}
		
		/// <summary>
		/// Загрузка изменений в БД
		/// </summary>
		public void LoadChanges()
		{
			//dynamic XMLReader = object1c.NewObject("ЧтениеXML");
			string patchToFile = Path.GetTempPath() + @"SA1C\In\Message" + baseInfo.Name + ".xml";
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
			InvokeMethod(obmenPlans, "ПрочитатьИзменения", messageRecord,10); //10 - количество элементов в транзакции
			InvokeMethod(messageRecord, "ЗакончитьЧтение", "");
			
			//отрубим все созданные СОМ
			Marshal.ReleaseComObject(loadXML); loadXML = null;
			Marshal.ReleaseComObject(obmenPlans); obmenPlans = null;
			Marshal.ReleaseComObject(messageRecord); messageRecord = null;
		}

		/// <summary>
		/// Выгрузка изменений из БД
		/// </summary>
		public void SaveChanges(string nameOfPlan, string codeOfNode)
		{
			//dynamic XMLSaver = object1c.NewObject("ЗаписьXML");
			string patchToFile = Path.GetTempPath() + @"SA1C\Out\Message" + baseInfo.Name + ".xml";
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
			InvokeMethod(obmenPlans, "ЗаписатьИзменения", messageRecord,10); //10 - количество элементов в транзакции
			InvokeMethod(messageRecord, "ЗакончитьЗапись", "");
			
			//отрубим все созданные СОМ
			Marshal.ReleaseComObject(saveXML); saveXML = null;
			Marshal.ReleaseComObject(obmenPlans); obmenPlans = null;
			Marshal.ReleaseComObject(plan); plan = null;
			Marshal.ReleaseComObject(node); node = null;
			Marshal.ReleaseComObject(messageRecord); messageRecord = null;
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
