using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using _1C;
using NLog;
using System.ServiceModel;

namespace SA1CService
{
	public partial class ServiceSA1C : IService1C, IFileTransfer,IWebServ
	{
		
		/// <summary>
		/// Упаковывает файл и получает его поток
		/// </summary>
		/// <param name="settingName">название настройки</param>
		/// <returns></returns>
		Stream GetCompressFileStream(string settingName){
			Logger logger = LogManager.GetCurrentClassLogger();
			FileStream fileData = null;
			string patchToFile = Path.GetTempPath() + @"SA1C\Out\Message" + settingName + ".xml";
			
			//архивируем файл сообщения
			FileInfo fi = new FileInfo(patchToFile);
			//проверим, есть ли доступ к файлу и если нет
			//ждем пока файл освободит другой процесс
			int wait = 0;
			while (wait<15)
			{
				try
				{
					FileStream fs = File.Open(patchToFile, FileMode.Open, FileAccess.Read, FileShare.Read);
					fs.Close();
					break; //выходим из бесконечного цикла
				}
				catch
				{
					Thread.Sleep(1000);
					logger.Error("Файл заблокирован");
					//ждать будем 15 секунд
					wait +=1;
				}
			}
			
			if(wait==15){
				//значит не открыли файл
				throw new Exception("Error open file "+patchToFile);
			}
			
			try{
				patchToFile = ZipFile.Compress(fi);
				//TODO: удаляем не архивированный файл
				//не удаляем для поддержки продолжения процесса
				//fi.Delete();
			}
			catch(Exception e){
				
				logger.Error("Arch or delete: " + e.Message);
				throw e;
			}
			
			
			if (!File.Exists(patchToFile))
			{
				
				logger.Error("Нет файла " + patchToFile);
				throw new Exception("Нет файла " + patchToFile);
			}

			try{
				fileData = File.Open(patchToFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch(Exception e){
				
				logger.Error("Open file: " + e.Message);
				throw e;
			}
			
			return fileData as Stream;
		}
		
		#region IFileTransfer Members
		/// <summary>
		/// Получает поток из файла выгрузки прописанного в настройке
		/// </summary>
		/// <param name="fileName">название настройки</param>
		/// <returns></returns>
		public Stream LoadFile(string settingName, long offset = 0)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			logger.Debug(settingName+": Передача файла");
			//LoadSettings();
			//var baseConfig = GetBaseConfig(settingName);
			//baseConfig.status = Status.GetFileFromServer;
			//SaveSettings();
			Stream temp = GetCompressFileStream(settingName);
			temp.Seek(offset,SeekOrigin.Begin);
			return temp;
			
		}

		/// <summary>
		/// Создает из потока файл
		/// </summary>
		/// <param name="stream">поток</param>
		public void UploadFile(FileData fileData)
		{
			Logger logger = LogManager.GetCurrentClassLogger();
			string patchToFile = Path.GetTempPath() + @"SA1C\In\Message" + fileData.settingName + ".xml.zip";

			try{
				Status status = GetCurrentStatus(fileData.settingName);
				
				if(status.currentPosInFile > 0 ){
					using (FileStream tempFile = File.Open(patchToFile,FileMode.Open))
					{
						//переместимся на прошлый обрыв в файле и в потоке
						tempFile.Seek(status.currentPosInFile,SeekOrigin.Begin);
						//fileData.fileByteStream.Seek(status.currentPosInFile,SeekOrigin.Begin);
						
						int numByteRead = 0;
						byte[] buffer = new byte[4096];
						numByteRead = fileData.fileByteStream.Read(buffer, 0, 4096);
						
						while (numByteRead != 0)
						{
							tempFile.Write(buffer, 0, numByteRead);
							SetCurrentStatus(fileData.settingName, Job.GetFileFromServer, JobStatus.Process,"",fileData.fileByteStream.Position);
							numByteRead = fileData.fileByteStream.Read(buffer, 0, 4096);
						}

						tempFile.Flush();
					}
				}
				else{
					using (FileStream tempFile = File.Create(patchToFile))
					{
						//logger.Error("до сеек");
						//на всякий случай переместимся в начало
						//fileData.fileByteStream.Seek(0,SeekOrigin.Begin);
						//logger.Error("после сеек");
						
						int numByteRead = 0;
						byte[] buffer = new byte[4096];
						numByteRead = fileData.fileByteStream.Read(buffer, 0, 4096);
						
						while (numByteRead != 0)
						{
							tempFile.Write(buffer, 0, numByteRead);
							SetCurrentStatus(fileData.settingName, Job.GetFileFromServer, JobStatus.Process,"",fileData.fileByteStream.Position);
							numByteRead = fileData.fileByteStream.Read(buffer, 0, 4096);
						}

						tempFile.Flush();
					}
					
				}
				
				SetCurrentStatus(fileData.settingName, Job.GetFileFromServer, JobStatus.Complite,"",0);
				
				FileInfo fi = new FileInfo(patchToFile);
				ZipFile.Decompress(fi);
			}
			catch(Exception e){
				
				logger.Error(e.Message);
				if(e.InnerException != null){
					logger.Error(e.InnerException.Message);
				}				
				SetCurrentStatus(fileData.settingName, Job.GetFileFromServer, JobStatus.Error,e.Message);
				throw e;
			}

		}

		#endregion
	}
}
