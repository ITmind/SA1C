using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;

namespace _1C
{
	public static class Helper1C
	{
		public static EnumVersion1C GetVersionFromFilePatch(string filePath)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug("Helper1C.GetVersionFromFilePatch ( "+filePath+" )");
			
			int index = filePath.IndexOf(@"\1Cv81\");
			if (index != -1)
			{
				return EnumVersion1C.v81;
			}

			index = filePath.IndexOf(@"\1Cv8\");
			if (index != -1)
			{
				return EnumVersion1C.v8;
			}

			return EnumVersion1C.v82;
		}

		/// <summary>
		/// Определяем базы по записям в файлах *.v8i
		/// </summary>
		/// <returns></returns>
		public static List<BaseInfo> GetBasesFromAddData()
		{
			List<BaseInfo> baseList = new List<BaseInfo>();
			BaseInfoCompater compater = new BaseInfoCompater();
			string[] filesPach = null;

			Logger logger = LogManager.GetCurrentClassLogger();
			
			
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.None);
			logger.Debug("Helper1C.GetBasesFromAddData: "+appData);
			
			try
			{
				//logger.Debug("Get files from: "+appData + "\\1c");
				filesPach = Directory.GetFiles(appData + "\\1c", "*.v8i", SearchOption.AllDirectories);
				//logger.Debug("Get files succes: "+filesPach.Length.ToString());
			}
			catch(Exception ex)
			{
				logger.Debug("Ошибка получения пути: "+ex.Message);
				//обработка ошибки
				return baseList;
			}

			foreach (string filePach in filesPach)
			{
				//logger.Debug("Парсим "+filePach);
				var file = File.OpenRead(filePach);
				var reader = new StreamReader(file);
				string nameBase = string.Empty;
				string connectString = string.Empty;
				string versionString = string.Empty;
				EnumVersion1C version = 0;
				BaseInfo _baseInfo = null;

				//logger.Debug("Начало чтения файла");
				while (!reader.EndOfStream)
				{
					
					var line = reader.ReadLine();
					//logger.Debug("Прочитана строка: "+line);

					
					if (line.IndexOf('[') > -1)
					{
						//logger.Debug("-- Это название базы");
						if(_baseInfo != null)
						{
							//logger.Debug("-- Объект _baseInfo уже создан");
							if (_baseInfo.Version1C == EnumVersion1C.NA)
							{
								_baseInfo.Version1C = GetVersionFromFilePatch(filePach);
								//logger.Debug("-- Версия 1С по пути: "+_baseInfo.Version1C.ToString());
							}
							//сначала проверим есть ли такая база в списке и если нет то добавлем
							if (!baseList.Contains(_baseInfo, compater))
							{
								//logger.Debug("-- Добавляем базу в список");
								baseList.Add(_baseInfo);
							}
							else{
								//logger.Debug("-- База уже есть в списоке. Ничего не делаем.");
							}
						}

						_baseInfo = new BaseInfo();
						_baseInfo.Name = line.Substring(1, line.Length - 2);

					}
					
					if (line.IndexOf("Version=") > -1)
					{
						//logger.Debug("-- Это версия 1С");
						versionString = line.Substring(8);
						switch (versionString)
						{
								case "8.1": version = EnumVersion1C.v81;
								break;
								case "8.2": version = EnumVersion1C.v82;
								break;
								default: version = 0;
								break;
						}
						if (version != 0)
						{
							_baseInfo.Version1C = version;
						}
						else
						{
							_baseInfo.Version1C = GetVersionFromFilePatch(filePach);
						}
					}
					
					if (line.IndexOf("Connect=") > -1)
					{
						//logger.Debug("-- Это строка соединения");
						connectString = line.Substring(8);
						_baseInfo.ConnectionString = connectString;
					}
				}
				
				//добавляем последнюю прочитанную базу в список, если ее еще там нет
				if (!baseList.Contains(_baseInfo, compater))
				{
					//logger.Debug("-- Добавляем базу в список");
					baseList.Add(_baseInfo);
				}
				else{
					//logger.Debug("-- База уже есть в списоке. Ничего не делаем.");
				}

			}

			return baseList;
		}
		
		public static string GetPathExe(EnumVersion1C version){
			string pathToExe = "";
			
			//будем использовать стандартные пути
			switch (version) {
				case EnumVersion1C.NA:
					break;
				case EnumVersion1C.v7:
					break;
				case EnumVersion1C.v8:
					pathToExe = @"C:\Program Files\1Cv8\bin\1cv8.exe";
					if(!File.Exists(pathToExe)){
						pathToExe = @"C:\\Program Files (x86)\1Cv8\bin\1cv8.exe";
					}
					break;
				case EnumVersion1C.v81:
					pathToExe = @"C:\Program Files\1Cv81\bin\1cv8.exe";
					if(!File.Exists(pathToExe)){
						pathToExe = @"C:\\Program Files (x86)\1Cv81\bin\1cv8.exe";
					}
					break;
				case EnumVersion1C.v82:
					pathToExe = @"C:\Program Files\1Cv82\common\1cestart.exe";
					if(!File.Exists(pathToExe)){
						pathToExe = @"C:\\Program Files (x86)\1Cv82\common\1cestart.exe";
					}
					break;
			}
			
			return pathToExe;
		}


		class BaseInfoCompater : IEqualityComparer<BaseInfo>
		{

			public bool Equals(BaseInfo x, BaseInfo y)
			{
				//Check whether the compared objects reference the same data.
				if (Object.ReferenceEquals(x, y)) return true;

				//Check whether any of the compared objects is null.
				if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
					return false;

				//Check whether the properties are equal.
				return x.ConnectionString == y.ConnectionString;
			}

			public int GetHashCode(BaseInfo obj)
			{
				throw new NotImplementedException();
			}
		}
	}
}