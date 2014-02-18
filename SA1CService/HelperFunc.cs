/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 15.03.2011
 * Время: 11:15
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace SA1CService
{
	/// <summary>
	/// Description of HelperFunc.
	/// </summary>
	public static class HelperFunc
	{
		public static bool IsWCFConfig(){
			bool result = false;
			string configFileName = Assembly.GetEntryAssembly().Location + ".config";
			if(File.Exists(configFileName)){
				try{
					XmlDocument document = new XmlDocument();
					document.Load(configFileName);
					XmlNode node = document.DocumentElement.SelectSingleNode(@"/configuration/system.serviceModel");
					if(node != null){
						result = true;
					}
				}
				catch{
					result = false;
				}
			}
			
			return result;
		}
	}
}
