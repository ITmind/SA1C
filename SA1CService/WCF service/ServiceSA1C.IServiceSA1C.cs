// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using _1C;
using NLog;

namespace SA1CService
{
	public partial class ServiceSA1C : IService1C, IFileTransfer,IWebServ
	{

		#region IService1C Members
		/// <summary>
		/// Выгрузка измененеий из 1Сv8
		/// </summary>
		/// <param name="settingsFile">Имя настройки</param>
		/// <returns>описание ошибки или "ok"</returns>
		public void SaveChanges(string settingName)
		{
			
			Logger logger = LogManager.GetCurrentClassLogger();
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": Начало выгрузки изменений");
			theEvent.Properties["SettingsName"] = settingName;
			logger.Log(theEvent);
			//logger.Debug(settingName+": Начало выгрузки изменений");
			
			var baseConfig = GetBaseConfig(settingName);
			SetCurrentStatus(settingName,Job.LocalSave, JobStatus.Process);
			OnExchangeProcess(GetCurrentStatus(settingName));
			
			using (_1Cv8 v8 = new _1Cv8(baseConfig.baseInfo))
			{

				try
				{
					v8.Connect();
					if(baseConfig.IsUniversalExchangeXML){
						v8.SaveUniversalXML(baseConfig.NameOfPlan, baseConfig.CodeOfNode,baseConfig.filenameRules);
					}
					else{
						v8.SaveChanges(baseConfig.NameOfPlan, baseConfig.CodeOfNode);
					}
					SetCurrentStatus(settingName,Job.LocalSave, JobStatus.Complite);
				}
				catch (Exception error)
				{
					string e = "";
					if (error.InnerException != null)
					{
						e = error.InnerException.Message;

					}
					else
					{
						e = error.Message;
					}
					//v8.Dispose();
					theEvent = new LogEventInfo(LogLevel.Error, logger.Name, e);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);
					//logger.Error(e);
					SetCurrentStatus(settingName,Job.LocalSave, JobStatus.Error, e);
				}
			}
			
			theEvent = new LogEventInfo(LogLevel.Info, logger.Name, settingName+": Конец выгрузки изменений");
			theEvent.Properties["SettingsName"] = settingName;
			logger.Log(theEvent);
			//logger.Info(settingName+": Конец выгрузки изменений");
			
			baseConfig = GetBaseConfig(settingName);
			baseConfig.LastExchangeDate = DateTime.Now;
			SaveSettings();
			
			OnExchangeProcess(GetCurrentStatus(settingName));
		}

		/// <summary>
		/// Загрузка измененеий в 1С
		/// </summary>
		/// <param name="settingsFile">Имя настройки</param>
		/// <returns>описание ошибки или "ok"</returns>
		public void LoadChanges(string settingName)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": Начало загрузки изменений");
			theEvent.Properties["SettingsName"] = settingName;
			logger.Log(theEvent);
			//logger.Debug();
			
			bool configChanged = false;
			bool isError = false;
			
			var baseConfig = GetBaseConfig(settingName);
			SetCurrentStatus(settingName,Job.LocalLoad, JobStatus.Process);
			OnExchangeProcess(GetCurrentStatus(settingName));
			
			
			using (_1Cv8 v8 = new _1Cv8(baseConfig.baseInfo))
			{

				try
				{
					//logger.Info("Connect");
					v8.Connect();
					//logger.Info("Load");
					if(baseConfig.IsUniversalExchangeXML){
						v8.LoadUniversalXML();
					}					
					else{
						v8.LoadChanges();
					}
					
				}
				catch (Exception error)
				{
					
					string e = "";
					if (error.InnerException != null)
					{
						e = error.InnerException.Message;
						//сделаем такую проверку, т.к. она быстрее и не может выдать ошибку
						if (e.Contains("обновление конфигурации"))
						{
							configChanged = true;
						}

					}
					else
					{
						e = error.Message;
					}
					
					isError = true;
					SetCurrentStatus(settingName,Job.LocalLoad, JobStatus.Error, e);
					theEvent = new LogEventInfo(LogLevel.Error, logger.Name, e);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);					
				}

			}
			
			//если требуется обновление конфигурации БД
			if(configChanged){
				theEvent = new LogEventInfo(LogLevel.Info, logger.Name, settingName+": Обновляем конфигурацию");
				theEvent.Properties["SettingsName"] = settingName;
				logger.Log(theEvent);
				//logger.Info(settingName+": Обновляем конфигурацию");
				_1Cv8 v8 = new _1Cv8(baseConfig.baseInfo);
				
				string isUpdate = v8.UpdateDB(config.DinamycUpdateDB,
				                              config.DisableUser);
				v8.Dispose();
				
				//и необходимо заного загрузить данные
				if(isUpdate == "ok"){
					LoadChanges(settingName);
					return; //выйдем
				}
				
				SetCurrentStatus(settingName,Job.LocalLoad, JobStatus.Error,isUpdate);
			}
			
			if(!isError){
				theEvent = new LogEventInfo(LogLevel.Info, logger.Name, settingName+": Конец загрузки изменений");
				theEvent.Properties["SettingsName"] = settingName;
				logger.Log(theEvent);
				//logger.Info(settingName+": Конец загрузки изменений");
				SetCurrentStatus(settingName,Job.LocalLoad, JobStatus.Complite);
			}
			
			baseConfig = GetBaseConfig(settingName);
			baseConfig.LastExchangeDate = DateTime.Now;
			SaveSettings();
			OnExchangeProcess(GetCurrentStatus(settingName));
		}

		/// <summary>
		/// Получить текущий статус
		/// </summary>
		/// <param name="settingName">Имя настройки</param>
		/// <param name="status">Текущее действие</param>
		public Status GetCurrentStatus(string settingName)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			//logger.Debug("Получаем статус "+settingName);
			Status _status = new Status();
			
			try{
				var baseConfig = GetBaseConfig(settingName);
				_status = baseConfig.status;
				LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": GetStatus job: "+_status.job+" jobstatus:"+_status.jobStatus+" description:"+_status.description);
				theEvent.Properties["SettingsName"] = settingName;
				logger.Log(theEvent);
			}
			catch(Exception e){
				LogEventInfo theEvent = new LogEventInfo(LogLevel.Error, logger.Name, settingName+": Ошибка получения статуса: "+e.Message);
				theEvent.Properties["SettingsName"] = settingName;
				logger.Log(theEvent);
				_status.job = Job.Exchange;
				_status.jobStatus = JobStatus.Error;
				_status.description = e.Message;
			}
			
			return _status;

		}

		public List<BaseConfig> GetAllSettings(){
			var baseConfig = from conf in config.basesConfig
				select conf;

			List<BaseConfig> result = new List<BaseConfig>(2);
			//BaseConfig[] result = new BaseConfig[baseConfig.Count()];
			//int index = 0;
			foreach (var conf in baseConfig) {
				result.Add(conf);
				//result[index] = conf;
				//index++;
			}
			
			return result.ToList();
		}
		
		public void SetCurrentStatus(string settingName, Job job, JobStatus jobStatus, string description = "", long currentPosInFile = -1){
			Logger logger = LogManager.GetCurrentClassLogger();
			var baseConfig = GetBaseConfig(settingName);
			baseConfig.status.job = job;
			baseConfig.status.jobStatus = jobStatus;
			baseConfig.status.description = description;
			if(currentPosInFile != -1){
				baseConfig.status.currentPosInFile = currentPosInFile;
			}
			SaveSettings();
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": SetStatus job: "+job+" jobstatus:"+jobStatus+" description:"+description);
			theEvent.Properties["SettingsName"] = settingName;
			logger.Log(theEvent);
			//logger.Debug(settingName+": SetStatus job: "+job+" jobstatus:"+jobStatus+" description:"+description);
		}
		
		/// <summary>
		/// Запусть с указанного действия
		/// </summary>
		/// <param name="settingName">Имя настройки</param>
		/// <param name="status">Текущее действие</param>
		public void ContinueProcess(string settingName, Job job)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": Start ContinueProcess");
			theEvent.Properties["SettingsName"] = settingName;
			logger.Log(theEvent);
			//logger.Debug(settingName+": Start ContinueProcess");

			var baseConfig = GetBaseConfig(settingName);
			baseConfig.status.job = job;
			baseConfig.status.jobStatus = JobStatus.Process;
            baseConfig.status.currentPosInFile = 0;
			SaveSettings();

			Process(settingName);
		}

		/// <summary>
		/// Продолжить передыдущее действие
		/// </summary>
		/// <param name="settingName">Имя настройки</param>
		public void Process(string settingName)
		{
			var baseConfig = GetBaseConfig(settingName);
			Status status = new Status();
			
			Logger logger = LogManager.GetCurrentClassLogger();
			LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": Start Process");
			theEvent.Properties["SettingsName"] = settingName;
			logger.Log(theEvent);
			//logger.Debug(settingName+": Start Process");
			//TODO
			int numRepeate = 0;
			while(numRepeate<=config.NumOperationRepeat){
				switch (baseConfig.status.job)
				{
					case Job.Exchange:
					case Job.LocalSave:
						if(baseConfig.IsSave){
							SaveChanges(settingName);
						}
						
						status = GetCurrentStatus(settingName);
						if(status.jobStatus != JobStatus.Error){
							SetCurrentStatus(settingName, Job.SendFileToServer,JobStatus.Process);
						}
						
						break;
						
					case Job.RemoteLoad:
						if(baseConfig.IsSave){
							RemoteLoadChanges(settingName);
						}
						
						status = GetCurrentStatus(settingName);
						if(status.jobStatus != JobStatus.Error){
							if(baseConfig.IsCentralDB){
								SetCurrentStatus(settingName, Job.RemoteSave, JobStatus.Process);
							}
							else{
								SetCurrentStatus(settingName, Job.Exchange, JobStatus.Complite);
							}
						}
						break;
						
					case Job.GetFileFromServer:
						if(baseConfig.IsLoad){
							LoadFileFromServer(settingName);
						}
						status = GetCurrentStatus(settingName);
						if(status.jobStatus != JobStatus.Error){
							SetCurrentStatus(settingName, Job.LocalLoad, JobStatus.Process);
						}
						break;
						
					case Job.LocalLoad:
						if(baseConfig.IsLoad){
							LoadChanges(settingName);
						}
						status = GetCurrentStatus(settingName);
						if(status.jobStatus != JobStatus.Error){
							if(baseConfig.IsCentralDB){
								SetCurrentStatus(settingName, Job.Exchange, JobStatus.Complite);
							}
							else{
								SetCurrentStatus(settingName, Job.LocalSave, JobStatus.Process);
							}
						}
						
						break;
						
					case Job.SendFileToServer:
						if(baseConfig.IsSave){
							UploadFileToServer(settingName);
						}
						status = GetCurrentStatus(settingName);
						if(status.jobStatus != JobStatus.Error){
							SetCurrentStatus(settingName, Job.RemoteLoad, JobStatus.Process);
						}
						break;
						
					case Job.RemoteSave:
						if(baseConfig.IsLoad){
							RemoteSaveChanges(settingName);
						}
						status = GetCurrentStatus(settingName);
						if(status.jobStatus != JobStatus.Error){
							SetCurrentStatus(settingName, Job.GetFileFromServer, JobStatus.Process);
						}
						break;
				}//switch
				
				//logger.Debug("Проверим, выполнилась ли задача");
				//status = GetCurrentStatus(settingName);
				if(status.jobStatus == JobStatus.Error){
					theEvent = new LogEventInfo(LogLevel.Error, logger.Name, status.job +" : "+status.description);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);

                    //если ошибка получения статуса, то никаких попыток более
                    if (status.job == Job.Exchange)
                    {
                        numRepeate = config.NumOperationRepeat + 1;
                    }
                    else
                    {
                        numRepeate += 1;
                    }
					//подождем секунду
					Thread.Sleep(4000);
					theEvent = new LogEventInfo(LogLevel.Warn, logger.Name, numRepeate.ToString() + " повтор задачи "+status.job);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);
				}
				else{
					numRepeate = config.NumOperationRepeat+1;
				}
			}//while
			
		}//func


		public void ExecuteExchange(string settingName, string isNewExchange)
		{
			if(isNewExchange=="1"){
				ExecuteExchange(settingName, true);
			}
			else{
				ExecuteExchange(settingName, false);
			}
		}
		/// <summary>
		/// Запуск обмена
		/// </summary>
		/// <param name="settingName"></param>
		public void ExecuteExchange(string settingName, bool isNewExchange = true)
		{
			var baseConfig = GetBaseConfig(settingName);
			Status status = new Status();
			Logger logger = LogManager.GetCurrentClassLogger();
			bool showError = false;
			
			try{
				if(isNewExchange){
					LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, logger.Name, "Запуск нового обмена по настройке "+settingName);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);
					//logger.Info("Запуск нового обмена по настройке "+settingName);
					if(baseConfig.IsCentralDB){
						ContinueProcess(settingName, Job.LocalSave);
						//ExecuteExchangeWithNotCB(settingName);
					}
					else{
						ContinueProcess(settingName, Job.RemoteSave);
						//ExecuteExchangeWithCB(settingName);
					}
				}
				else{
					LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, logger.Name, "Продолжение обмена по настройке "+settingName);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);
					//logger.Info("Продолжение обмена по настройке "+settingName);
					Process(settingName); //продолжим с последней задачи
				}
				
				status = GetCurrentStatus(settingName);
				
				while((status.job != Job.Exchange && status.jobStatus != JobStatus.Complite ) &&
				      status.jobStatus != JobStatus.Error){
					
					LogEventInfo theEvent = new LogEventInfo(LogLevel.Debug, logger.Name, settingName+": Выполнение задачи "+status.job);
					theEvent.Properties["SettingsName"] = settingName;
					logger.Log(theEvent);
					
					//logger.Debug(settingName+": Выполнение задачи "+status.job);
					
					Process(settingName);
					status = GetCurrentStatus(settingName);
				}
			}
			catch(Exception error){
				showError = true;
				status = GetCurrentStatus(settingName);
				status.jobStatus = JobStatus.Error;
				
				//если ошибка на сервере то попробуем получить описание с сервера
				if(error.Message.Contains("IncludeExceptionDetailInFaults")){
					Status tempStatus = RemoteGetCurrentStatus(settingName);
					status.description = tempStatus.description;
				}
				else{
					status.description = error.Message;
				}
				
				SetCurrentStatus(settingName, status.job, status.jobStatus, status.description);
			}
			
			if(status.job == Job.Exchange && status.jobStatus == JobStatus.Complite){
				if(config.SendSuccessMessage){
					Mail.SendMail(config.Email,"Обмен по настройке "+settingName+" завершен успешно","Обмен по настройке "+settingName+" завершен успешно");
				}
				
				baseConfig = GetBaseConfig(settingName);
				baseConfig.LastExchangeDate = DateTime.Now;
				SaveSettings();
				
				OnExchangeProcess(status);
				
				LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, logger.Name, "Обмен по настройке "+settingName+" завершен успешно");
				theEvent.Properties["SettingsName"] = settingName;
				logger.Log(theEvent);
				
				//logger.Info(("Обмен по настройке "+settingName+" завершен успешно");
			}
			
			else{
				LogEventInfo theEvent = new LogEventInfo(LogLevel.Error, logger.Name, settingName+": Ошибка: " + status.description);
				theEvent.Properties["SettingsName"] = settingName;
				logger.Log(theEvent);
				
				//logger.Error(settingName+": Ошибка: " + status.description);
				
				if(config.SendErrorMessage){
					Mail.SendMail(config.Email,"Ошибка обмена по настройке "+settingName,"Ошибка: " + status.description);
				}
				
				if(showError){
					OnExchangeProcess(status);
				}
			}
			
			
			
		}


		#endregion

	}
}
