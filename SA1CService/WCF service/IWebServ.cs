/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 02.06.2011
 * Время: 17:41
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SA1CService
{
	/// <summary>
	/// Description of IWebServ.
	/// </summary>
	
	[ServiceContract]
	public interface IWebServ
	{
		[OperationContract]
		[WebGet(UriTemplate = "getallsettings", ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		List<BaseConfig> GetAllSettings();
		
		[OperationContract(IsOneWay = true)]
		[WebGet(UriTemplate = "/execute?setting={settingName}&isNew={isNewExchange}")]
        void ExecuteExchange(string settingName, string isNewExchange);

	}
}

