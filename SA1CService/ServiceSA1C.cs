using System;
using System.IO;
using System.Threading;
using _1C;
using NLog;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace SA1CService
{
	public class ExchangeProcessEventArgs : EventArgs
	{
		public Status status;
		
		public ExchangeProcessEventArgs(Status _status)
		{
			this.status = _status;
		}
		
	}
	
	[Serializable]
	public partial class ServiceSA1C : IService1C, IFileTransfer
	{
		//TODO Добавить логирование во все функции
		public SA1CConfig config { get; set; }
		//Logger logger = LogManager.GetCurrentClassLogger();

		#region Members

		public ServiceSA1C()
		{
			config = new SA1CConfig();
			Logger logger = LogManager.GetCurrentClassLogger();
			//logger.Debug("Создан экземпляр ServiceSA1C");
			
			//проверим каталоги для обмена, если их нет, то создадим
			string OutDirectory = Path.GetTempPath() + @"SA1C\Out\";
			string InDirectory = Path.GetTempPath() + @"SA1C\In\";
			
			if(!Directory.Exists(OutDirectory)){
				Directory.CreateDirectory(OutDirectory);
			}
			
			if(!Directory.Exists(InDirectory)){
				Directory.CreateDirectory(InDirectory);
			}
			
			LoadSettings();
		}

		
		public delegate void ExchangeProcessEventHandler(object sender, ExchangeProcessEventArgs e);
		
		/// <summary>
		/// Вызывается в ходе процесса обмена
		/// </summary>
		public event ExchangeProcessEventHandler ExchangeProcess;
		
		void OnExchangeProcess(Status status)
		{
			ExchangeProcessEventHandler handler = this.ExchangeProcess;
			if (handler != null)
				handler(this, new ExchangeProcessEventArgs(status));
		}
		
		private BaseConfig GetBaseConfig(string settingName)
		{
			LoadSettings();
			var baseConfig = from conf in config.basesConfig
				where conf.Name == settingName
				select conf;

			if (baseConfig.Count() == 0)
			{
				throw new Exception("Не найдена настройка: " + settingName);
			}

			return baseConfig.First();
		}

		//private EmailSetting GetEmailSetting()
		//{
		//	LoadSettings();
		//	return config.Email;
		//}
		
		/// <summary>
		/// Архивирование БД
		/// </summary>
		/// <param name="settingName">название настройки</param>
		private void Arch(string settingName)
		{
			
//			var baseConfig = GetBaseConfig(settingName);
//			
//			if (baseConfig.IsArch)
//			{
//				_1Cv8 v8 = new _1Cv8(baseConfig.baseInfo);
//				v8.Arch(baseConfig.PatchToFileArch);
//			}
		}

		/// <summary>
		/// Загрузка настроек на стороне сервера
		/// </summary>
		/// <param name="name">Имя настройки</param>
		/// <returns>класс содержащий настройки</returns>
		public void LoadSettings()
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			//logger.Debug("LoadSettings from "+Directory.GetCurrentDirectory());
			
			if (File.Exists("set.xml"))
			{
				XmlSerializer mySerializer = new XmlSerializer(typeof(SA1CConfig));
				FileStream myFileStream = new FileStream("set.xml", FileMode.Open);
				config = (SA1CConfig)mySerializer.Deserialize(myFileStream);
				myFileStream.Close();
				//поддержка старой версии настроек
				if(config.Version == "1"){
					foreach(BaseConfig baseConfig in config.basesConfig){
						//зашифруем пароли
						baseConfig.baseInfo.SetPassword(baseConfig.baseInfo.Password);
						//и разделим IP и Port
						int index = baseConfig.IP.LastIndexOf(':');
						if(index == -1) continue;
						int length = baseConfig.IP.Length-1;
						baseConfig.Port = baseConfig.IP.Substring(index+1,length-index);
						baseConfig.IP = baseConfig.IP.Substring(0,index);
					}
					SaveSettings();
				}
			}
		}

		public void SaveSettings()
		{
			XmlSerializer mySerializer = new XmlSerializer(typeof(SA1CConfig));
			StreamWriter myWriter = new StreamWriter("set.xml");
			config.Version = "2";
			mySerializer.Serialize(myWriter, config);
			myWriter.Close();
		}

		/// <summary>
		/// Загрузка изменений в удаленной базе
		/// </summary>
		/// <param name="settingName">Название настройки обмена</param>
		public void RemoteLoadChanges(string settingName)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug(settingName+": Начало загрузки изменений в удаленной базе");
			SetCurrentStatus(settingName, Job.RemoteLoad, JobStatus.Process);
			OnExchangeProcess(GetCurrentStatus(settingName));
			
			var baseConfig = GetBaseConfig(settingName);
			ChannelFactory<IService1C> factory;
			
			Status remoteStatus = RemoteGetCurrentStatus(settingName);
			if(remoteStatus.jobStatus == JobStatus.Error && remoteStatus.description.Contains("Не найдена настройка")){
				throw new Exception(remoteStatus.description);
			}
			
			//попробуем загрузить из файла конфигурации
			try{
				factory = new ChannelFactory<IService1C>("IService1C");
			}
			catch{
				//если нет то создаем вручную
				
				NetTcpBinding TCPbinding = new NetTcpBinding(SecurityMode.None);
				TCPbinding.CloseTimeout = TimeSpan.FromHours(0.5);
				TCPbinding.OpenTimeout = TimeSpan.FromHours(0.5);
				TCPbinding.SendTimeout = TimeSpan.FromHours(13);
				TCPbinding.TransferMode = TransferMode.Buffered;
				TCPbinding.MaxReceivedMessageSize = 429496729;
				
				EndpointAddress endpointAdress = new EndpointAddress(config.HostSetting.Protocol + @"://" + baseConfig.ServerAdress + "/Service1C");
				factory = new ChannelFactory<IService1C>(TCPbinding,endpointAdress);
			}

			IService1C channel = factory.CreateChannel();

			channel.LoadChanges(settingName);
			factory.Abort();//.Close();
			
			//ждем пока не выполнится задача
			Status _status = WaitServerJob(settingName);
			
			if(_status.jobStatus == JobStatus.Complite){
				logger.Debug(settingName+": Конец загрузки изменений в удаленной базе");
				SetCurrentStatus(settingName, Job.RemoteLoad, JobStatus.Complite);
			}
			else{
				logger.Error(settingName+": Ошибка загрузки изменений в удаленной базе");
				SetCurrentStatus(settingName, Job.RemoteLoad, JobStatus.Error, _status.description);
			}
			
			OnExchangeProcess(GetCurrentStatus(settingName));

		}

		/// <summary>
		/// Выгрузка изменений из удаленной базы
		/// </summary>
		/// <param name="settingName">Название настройки обмена</param>
		public void RemoteSaveChanges(string settingName)
		{
			SetCurrentStatus(settingName, Job.RemoteSave, JobStatus.Process);
			OnExchangeProcess(GetCurrentStatus(settingName));
			
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug(settingName+": Начало выгрузки изменений в удаленной базе");
			var baseConfig = GetBaseConfig(settingName);
			ChannelFactory<IService1C> factory;

			Status remoteStatus = RemoteGetCurrentStatus(settingName);
			if(remoteStatus.jobStatus == JobStatus.Error && remoteStatus.description.Contains("Не найдена настройка")){
				throw new Exception(remoteStatus.description);
			}
			
			//попробуем загрузить из файла конфигурации
			try{
				factory = new ChannelFactory<IService1C>("IService1C");
			}
			catch{
				//если нет то создаем вручную
				
				NetTcpBinding TCPbinding = new NetTcpBinding(SecurityMode.None);
				TCPbinding.CloseTimeout = TimeSpan.FromHours(0.5);
				TCPbinding.OpenTimeout = TimeSpan.FromHours(0.5);
				TCPbinding.SendTimeout = TimeSpan.FromHours(13);
				TCPbinding.TransferMode = TransferMode.Buffered;
				TCPbinding.MaxReceivedMessageSize = 429496729;
				
				EndpointAddress endpointAdress = new EndpointAddress(config.HostSetting.Protocol + @"://" + baseConfig.ServerAdress + "/Service1C");
				factory = new ChannelFactory<IService1C>(TCPbinding,endpointAdress);
				
				//factory.Endpoint.ListenUri = factory.Endpoint.Address.Uri;
				//factory.Endpoint.Binding = TCPbinding;
				//factory.Endpoint.Contract = new ContractDescription("SA1CService.IService1C");
			}
			
			IService1C channel = factory.CreateChannel();
			
			channel.SaveChanges(settingName);

			factory.Abort();//.Close();
			
			//ждем пока не выполнится задача
			Status _status = WaitServerJob(settingName);
			
			if(_status.jobStatus == JobStatus.Complite){
				logger.Debug(settingName+": Конец выгрзки изменений в удаленной базе");
				SetCurrentStatus(settingName, Job.RemoteSave, JobStatus.Complite);
			}
			else{
				logger.Error(settingName+": Ошибка выгрузки изменений в удаленной базе");
				SetCurrentStatus(settingName, Job.RemoteSave, JobStatus.Error, _status.description);
			}
			
			OnExchangeProcess(GetCurrentStatus(settingName));
		}

		/// <summary>
		/// Загрузка изменений в удаленной базе
		/// </summary>
		/// <param name="settingName">Название настройки обмена</param>
		public Status RemoteGetCurrentStatus(string settingName)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug(settingName+": Получаем текущий статус на сревере");
			var baseConfig = GetBaseConfig(settingName);
			ChannelFactory<IService1C> factory;
			
			Status request = new Status();
			try{
				//попробуем загрузить из файла конфигурации
				try{
					factory = new ChannelFactory<IService1C>("IService1C");
				}
				catch{
					//если нет то создаем вручную
					
					NetTcpBinding TCPbinding = new NetTcpBinding(SecurityMode.None);
					TCPbinding.CloseTimeout = TimeSpan.FromHours(0.5);
					TCPbinding.OpenTimeout = TimeSpan.FromHours(0.5);
					TCPbinding.SendTimeout = TimeSpan.FromHours(13);
					TCPbinding.TransferMode = TransferMode.Buffered;
					TCPbinding.MaxReceivedMessageSize = 429496729;
					
					EndpointAddress endpointAdress = new EndpointAddress(config.HostSetting.Protocol + @"://" + baseConfig.ServerAdress + "/Service1C");
					factory = new ChannelFactory<IService1C>(TCPbinding,endpointAdress);
				}

				IService1C channel = factory.CreateChannel();

				request = channel.GetCurrentStatus(settingName);
				factory.Close();
				
				logger.Debug(settingName+": Конец получения текущего статуса на сревере");
			}
			catch{
				request.job = Job.Exchange;
				request.jobStatus = JobStatus.Error;
				request.description = settingName+": Ошибка получения статуса на сервере";
			}

			return request;


		}
		
		public Status WaitServerJob(string settingName){
			Status _status = RemoteGetCurrentStatus(settingName);
			while(_status.jobStatus == JobStatus.Process){
				Thread.Sleep(30000); //подождем 30 секунд
				_status = RemoteGetCurrentStatus(settingName);
			}
			return _status;
		}
		
		/// <summary>
		/// Прием файла с сервера
		/// </summary>
		public void LoadFileFromServer(string settingName)
		{

			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug(settingName+": Начало приема файла с сервера");

			SetCurrentStatus(settingName, Job.GetFileFromServer, JobStatus.Process);
			OnExchangeProcess(GetCurrentStatus(settingName));
			
			var baseConfig = GetBaseConfig(settingName);
			ChannelFactory<IFileTransfer> factory;

			Status remoteStatus = RemoteGetCurrentStatus(settingName);
			if(remoteStatus.jobStatus == JobStatus.Error && remoteStatus.description.Contains("Не найдена настройка")){
				throw new Exception(remoteStatus.description);
			}

			//попробуем загрузить из файла конфигурации
			try{
				factory = new ChannelFactory<IFileTransfer>("IFileTransfer");
			}
			catch{
				//если нет то создаем вручную
				
				NetTcpBinding streamBinding = new NetTcpBinding(SecurityMode.None);
				streamBinding.CloseTimeout = TimeSpan.FromHours(0.5);
				streamBinding.OpenTimeout = TimeSpan.FromHours(0.5);
				streamBinding.SendTimeout = TimeSpan.FromHours(13);
				streamBinding.TransferMode = TransferMode.Buffered;
				streamBinding.MaxReceivedMessageSize = 429496729;
				
				EndpointAddress endpointAdress = new EndpointAddress(config.HostSetting.Protocol + @"://" + baseConfig.ServerAdress + "/FileTransfer");
				factory = new ChannelFactory<IFileTransfer>(streamBinding,endpointAdress);
			}
			
			
			//string patchToFile = Path.GetTempPath() + @"SA1C\In\Message" + settingName + ".xml.zip";

			IFileTransfer channel = factory.CreateChannel();

			long offset = 0;
			Status status = GetCurrentStatus(settingName);
			if(status.currentPosInFile > 0 ){
				offset = status.currentPosInFile;
			}
			
			using (Stream fileData = channel.LoadFile(settingName,offset))
			{
				FileData sendData = new FileData();
				sendData.settingName = settingName;
				sendData.fileByteStream = fileData;
				
				UploadFile(sendData);
				//using (FileStream tempFile = File.Create(patchToFile))
				//{
				//
				//	fileData.CopyTo(tempFile, 4096);
				//}
			}
			
			factory.Close();

			//разархивируем принятый файл
			//FileInfo fi = new FileInfo(patchToFile);
			//ZipFile.Decompress(fi);
			//TODO: удаляем архив
			//fi.Delete();
			
			logger.Debug(settingName+": Конец приема файла с сервера");
			
			SetCurrentStatus(settingName, Job.GetFileFromServer, JobStatus.Complite,"",0);
			OnExchangeProcess(GetCurrentStatus(settingName));
		}

		/// <summary>
		/// Отправка файла на сервер
		/// </summary>
		/// <param name="settingName">название настройки</param>
		/// <param name="Continue">продолжить с момента разрыва</param>
		public void UploadFileToServer(string settingName)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug(settingName+": Начало отправки файла на сервер");
			var baseConfig = GetBaseConfig(settingName);
			ChannelFactory<IFileTransfer> factory;
			
			SetCurrentStatus(settingName, Job.SendFileToServer, JobStatus.Process);
			OnExchangeProcess(GetCurrentStatus(settingName));

			Status remoteStatus = RemoteGetCurrentStatus(settingName);
			if(remoteStatus.jobStatus == JobStatus.Error && remoteStatus.description.Contains("Не найдена настройка")){
				throw new Exception(remoteStatus.description);
			}
			
			//закорем поток после передачи
			using(FileData sendData = new FileData()){
				sendData.settingName = settingName;
				sendData.fileByteStream = GetCompressFileStream(settingName);

				//попробуем загрузить из файла конфигурации
				try{
					factory = new ChannelFactory<IFileTransfer>("IFileTransfer");
				}
				catch{
					//если нет то создаем вручную
					
					NetTcpBinding streamBinding = new NetTcpBinding(SecurityMode.None);
					streamBinding.CloseTimeout = TimeSpan.FromHours(0.5);
					streamBinding.OpenTimeout = TimeSpan.FromHours(0.5);
					streamBinding.SendTimeout = TimeSpan.FromHours(13);
					streamBinding.TransferMode = TransferMode.Buffered;
					streamBinding.MaxReceivedMessageSize = 429496729;
					
					EndpointAddress endpointAdress = new EndpointAddress(config.HostSetting.Protocol + @"://" + baseConfig.ServerAdress + "/FileTransfer");
					factory = new ChannelFactory<IFileTransfer>(streamBinding,endpointAdress);
				}

				IFileTransfer channel = factory.CreateChannel();
				if(remoteStatus.currentPosInFile > 0 ){
					sendData.fileByteStream.Seek(remoteStatus.currentPosInFile,SeekOrigin.Begin);
				}
				
				channel.UploadFile(sendData);
				factory.Close();
			}
			
			logger.Debug(settingName+": Конец отправки файла на сервер");
			SetCurrentStatus(settingName, Job.SendFileToServer, JobStatus.Complite,"",0);
			OnExchangeProcess(GetCurrentStatus(settingName));
		}


		/// <summary>
		/// Продолжить обмен с последенего состояния
		/// </summary>
		/// <param name="settingName"></param>
		/// <param name="status"></param>
		private void ProcessOnServer(string settingName, Status status)
		{
//			Logger logger = LogManager.GetCurrentClassLogger();
//			logger.Info("Продолжение обмена с "+ status.ToString());
//			var baseConfig = GetBaseConfig(settingName);
//
//			ChannelFactory<IService1C> factory;
//
//			//попробуем загрузить из файла конфигурации
//			try{
//				factory = new ChannelFactory<IService1C>("IService1C");
//			}
//			catch{
//				//если нет то создаем вручную
//
//				NetTcpBinding TCPbinding = new NetTcpBinding(SecurityMode.None);
//				TCPbinding.CloseTimeout = TimeSpan.FromHours(0.5);
//				TCPbinding.OpenTimeout = TimeSpan.FromHours(0.5);
//				TCPbinding.SendTimeout = TimeSpan.FromHours(3);
//				TCPbinding.TransferMode = TransferMode.Buffered;
//				TCPbinding.MaxReceivedMessageSize = 429496729;
//
//				EndpointAddress endpointAdress = new EndpointAddress(config.HostSetting.Protocol + @"://" + baseConfig.ServerAdress + "/Service1C");
//				factory = new ChannelFactory<IService1C>(TCPbinding,endpointAdress);
//			}
//
//			IService1C channel = factory.CreateChannel();
//
//			channel.ContinueProcess(settingName, status);
//			factory.Close();
		}

		#endregion

	}
}
