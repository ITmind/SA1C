using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using _1C;

namespace SA1CService
{
	//[Serializable]
	[DataContract]
	public class Status
	{
		[DataMember]
		public Job job {get;set;}		
		[DataMember]
		public JobStatus jobStatus {get;set;}
		[DataMember]
		public string description {get;set;}
		[DataMember]
		public long currentPosInFile {get;set;}
		
		public Status(){
			job = Job.Exchange;
			jobStatus = JobStatus.Complite;
			description = "";
			currentPosInFile = 0;
		}
	}
	
	[DataContract]
	public class Sheduler{
		public Sheduler(){
			Expression = "* * * * * *";
		}		
		[DataMember]
		public bool isEnable { get; set; }		
		[DataMember]
		public string Expression{ get;set;}
	}
		
	[DataContract]
	public class BaseConfig
	{
		public BaseConfig()
		{
			baseInfo = new BaseInfo();
			status = new Status();
			JobSheduler = new Sheduler();
			baseInfo.Name = "Новая настройка";
			filenameRules = "";
			Name = "Новая настройка";
			IsLoad = true;
			IsSave = true;
			LastExchangeDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
		}

		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Адресс сервера IP:Port
		/// </summary>
		[DataMember]
		public string IP { get; set; }
		[DataMember]
		public string Port { get; set; }

		public string ServerAdress {
			get{
				StringBuilder sb = new StringBuilder(IP);
				sb.Append(":");
				sb.Append(Port);
				return sb.ToString();
			}
		}
		//план обмен по которому делаем обмен
		[DataMember]
		public string NameOfPlan { get; set; }
		
		//код узла с которым делаем обмен
		[DataMember]
		public string CodeOfNode { get; set; }
		
		//public string PatchToFileArch { get; set; }
		
		//public bool IsArch { get; set; }
		
		public bool IsUniversalExchangeXML {
			get{
				if(filenameRules == ""){
					return false;
				}
				else{
					return true;
				}
			}
				
		}
		[DataMember]
		public string filenameRules {get;set;}
		[DataMember]
		public bool IsSave {get;set;}
		[DataMember]
		public bool IsLoad {get;set;}	
		[DataMember]
		public BaseInfo baseInfo { get; set; }
		[DataMember]
		public Status status {get; set; }
		[DataMember]
		public DateTime LastExchangeDate { get; set; }
		[DataMember]
		public bool IsCentralDB { get; set; }
		[DataMember]
		public Sheduler JobSheduler{ get;set;}
	}

	[DataContract]
	public class EmailSetting{
		[DataMember]
		public string SMTPServer{ get; set;}
		[DataMember]
		public int Port { get; set;}
		[DataMember]
		public string User{ get; set;}
		[DataMember]
		public string Password{ get; set;}
		//адресса должны быть разделены запятыми
		[DataMember]
		public string MailTo{ get; set;}
		[DataMember]
		public bool EnableSsl {get;set;}
		
		public void SetPassword(string password)
		{
			if(password.Trim() == String.Empty){
				Password = "";
			}
			else{
				Password = Crypto.EncryptStringAES(password, "78h342nb");
			}
			
		}

		public string GetPassword()
		{
			if(Password.Trim() == String.Empty){
				return "";
			}
			else{
				return Crypto.DecryptStringAES(Password, "78h342nb");
			}
		}
		
	}
	
	[DataContract]
	public class ServiceHostSetting{
		[DataMember]
		public string Protocol { get; set;}
		[DataMember]
		public string BaseIP{ get; set;}
		[DataMember]
		public int Port { get; set;}
		[DataMember]
		public int PortWebService { get; set;}
		[DataMember]
		public int PortHTTPServer { get; set;}
		
		public string BaseAdress{
			get{
				StringBuilder sb = new StringBuilder();
				sb.Append(Protocol);
				sb.Append(@"://");
				sb.Append(BaseIP);
				sb.Append(@":");
				sb.Append(Port.ToString());
				sb.Append(@"/");
				return sb.ToString();
			}
		}
		
		public string BaseAdressHttp{
			get{
				StringBuilder sb = new StringBuilder();
				sb.Append(Uri.UriSchemeHttp);
				sb.Append(@"://");
				sb.Append(BaseIP);
				sb.Append(@":");
				sb.Append((PortWebService).ToString());
				sb.Append(@"/");
				return sb.ToString();
			}
		}
		
		public ServiceHostSetting(){
			Port = 8001;
			Protocol = Uri.UriSchemeNetTcp;
			BaseIP = "localhost";
			PortWebService = Port+1;
			PortHTTPServer = PortWebService+1;
		}
	}
	
	[Serializable]
	public class SA1CConfig
	{
		public string Version{ get; set; }
		public bool CheckUpdate { get; set; }
		
		//параметры рассылки
		public bool SendErrorMessage { get; set; }
		public bool SendSuccessMessage { get; set; }
		public EmailSetting Email { get; set;}
		//параметры обновления конфиуграции БД
		public TypeKillUsers DisableUser { get; set;}
		public bool DinamycUpdateDB { get; set;}
		public int NumOperationRepeat {get; set;}
		public int NumExchangeRepeat {get; set;}
		
		//параметры сервиса
		public ServiceHostSetting HostSetting { get; set;}
		
		public List<BaseConfig> basesConfig { get; set; }

		public SA1CConfig()
		{
			basesConfig = new List<BaseConfig>();
			Version = "1";
			CheckUpdate = true;
			Email = new EmailSetting();
			HostSetting = new ServiceHostSetting();
			DisableUser = TypeKillUsers.None;
			DinamycUpdateDB = true;
			NumOperationRepeat = 5;
			NumExchangeRepeat = 0;
		}

		public void AddConfig(BaseConfig baseConfig)
		{
			basesConfig.Add(baseConfig);
			AddConfigEventHandler handler = this.AddConfigEvent;
			if (handler != null)
				handler(this, new AddConfigEventArgs(baseConfig));
		}

		/// <summary>
		/// Сообщает, что добавлена настройка
		/// </summary>
		public delegate void AddConfigEventHandler(object sender, AddConfigEventArgs e);

		public event AddConfigEventHandler AddConfigEvent;

	}

	public class AddConfigEventArgs:EventArgs
	{
		public AddConfigEventArgs(BaseConfig baseConfig)
		{
			this.baseConfig = baseConfig;
		}

		public BaseConfig baseConfig;
	}
}
