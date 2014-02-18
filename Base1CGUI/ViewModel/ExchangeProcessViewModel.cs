/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 02/03/2011
 * Время: 17:00
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using SA1CService;

namespace Base1CGUI.ViewModel
{
	/// <summary>
	/// Description of ExchangeProcessViewModel.
	/// </summary>
	public class ExchangeProcessViewModel: WorkspaceViewModel
	{
		ServiceSA1C _serviceSA1C;
		BaseConfig _baseConfig;
		ObservableCollection<string> _logs;
		Thread exchangeTread;
		int _progressBarValue;
		
		public ExchangeProcessViewModel(WorkspaceViewModel mainWindow, ServiceSA1C serviceSA1C, BaseConfig baseConfig)
		{
			//Загрузим актуальные настройки
			
			base.MainWindow = mainWindow;
			_serviceSA1C = serviceSA1C;
			_baseConfig = baseConfig;
			_logs = new ObservableCollection<string>();
			ProgressBarValue = 0;
			DisplayName = _baseConfig.Name;
			
			//подписываемся на события
			_serviceSA1C.ExchangeProcess += serviceSA1C_ExchangeProcess;
			
			bool isNewExchange = true;
			//опрделяем, нужно ли продолжать обмен или начать заново
			if(baseConfig.status.job != Job.Exchange && baseConfig.status.jobStatus != JobStatus.Complite){
				var result = MessageBox.Show("При прошлом запуске обмен не был завешен. Продолжить обмен? \n" +
				                             "(нет - начать обмен заново)",
				                             "Обмен",
				                             MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
				{
					isNewExchange = false;
				}
			}
			
			ThreadStart start = () => _serviceSA1C.ExecuteExchange(_baseConfig.Name, isNewExchange);

			exchangeTread =  new Thread(start);
			//завершаем поток принудительно при завершении основного потока
			exchangeTread.IsBackground = true;
			exchangeTread.Start();
			
		}

		
		//обработчик события
		public void serviceSA1C_ExchangeProcess(object sender, ExchangeProcessEventArgs e)
		{
			
			if(App.Current.Dispatcher.CheckAccess()){
				
				if(e.status.jobStatus != JobStatus.Error){
					Commands.Clear();
					Commands.Add(new CommandViewModel(
						"Прервать",
						new RelayCommand(param => Abort())));
					
					switch(e.status.job){
						case Job.LocalSave:
							if(e.status.jobStatus == JobStatus.Process){
								if(!_logs.Contains("Выгрузка данных из локальной БД.")){
									_logs.Add("Выгрузка данных из локальной БД.");
								}
								else{
									_logs.Add("Повторная попытка.");
								}
								
							}
							else if(e.status.jobStatus == JobStatus.Complite) {
								_logs[_logs.Count-1]=_logs[_logs.Count-1]+@" ОК";
								ProgressBarValue += 10;
							}
							
							break;
						case Job.GetFileFromServer:
							if(e.status.jobStatus == JobStatus.Process){
								if(!_logs.Contains("Получение файла с сервера.")){
									_logs.Add("Получение файла с сервера.");
								}
								else{
									_logs.Add("Повторная попытка.");
								}
							}
							else if(e.status.jobStatus == JobStatus.Complite){
								_logs[_logs.Count-1]=_logs[_logs.Count-1]+@" ОК";
								ProgressBarValue += 10;
							}
							break;
						case Job.RemoteLoad:
							if(e.status.jobStatus == JobStatus.Process){
								if(!_logs.Contains("Загрузка данных в удаленной БД.")){
									_logs.Add("Загрузка данных в удаленной БД.");
								}
								else{
									_logs.Add("Повторная попытка.");
								}
							}
							else if(e.status.jobStatus == JobStatus.Complite){
								_logs[_logs.Count-1]=_logs[_logs.Count-1]+@" ОК";
								ProgressBarValue += 10;
							}
							break;
						case Job.RemoteSave:
							if(e.status.jobStatus == JobStatus.Process){
								if(!_logs.Contains("Выгрузка данных из удаленной БД.")){
									_logs.Add("Выгрузка данных из удаленной БД.");
								}
								else{
									_logs.Add("Повторная попытка.");
								}
							}
							else if(e.status.jobStatus == JobStatus.Complite){
								_logs[_logs.Count-1]=_logs[_logs.Count-1]+@" ОК";
								ProgressBarValue += 10;
							}
							break;
						case Job.SendFileToServer:
							if(e.status.jobStatus == JobStatus.Process){
								if(!_logs.Contains("Передача файла на сервер.")){
									_logs.Add("Передача файла на сервер.");
								}
								else{
									_logs.Add("Повторная попытка.");
								}
							}
							else if(e.status.jobStatus == JobStatus.Complite){
								_logs[_logs.Count-1]=_logs[_logs.Count-1]+@" ОК";
								ProgressBarValue += 10;
							}
							break;
						case Job.LocalLoad:
							if(e.status.jobStatus == JobStatus.Process){
								if(!_logs.Contains("Загрузка данных в локальную БД.")){
									_logs.Add("Загрузка данных в локальную БД.");
								}
								else{
									_logs.Add("Повторная загрузка.");
								}
							}
							else if(e.status.jobStatus == JobStatus.Complite){
								_logs[_logs.Count-1]=_logs[_logs.Count-1]+@" ОК";
								ProgressBarValue += 10;
							}
							break;
						case Job.Exchange:
							if(e.status.jobStatus == JobStatus.Process){
								//_logs.Add("Загрузка данных в локальную БД.");
							}
							else if(e.status.jobStatus == JobStatus.Complite){
								_logs.Add("ОБМЕН ЗАВЕРШЕН УСПЕШНО");
								Commands.Clear();
								Commands.Add(new CommandViewModel(
									"Назад",
									this.CloseCommand));
							}
							
							break;
					}
				}
				
				else{
					if(!_logs.Contains(e.status.description)){
						_logs.Add("ОШИБКА:");
						_logs.Add(e.status.description);
					}
					ProgressBarValue = 0;
					Commands.Clear();
					Commands.Add(new CommandViewModel(
						"Назад",
						this.CloseCommand));
				}
				
				
			}
			else{
				//если мы в другом потоке, то вызовем эту же процедуру из потока UI
				App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send,
				                                   new Action( () => {serviceSA1C_ExchangeProcess(sender,e); } )
				                                  );
			}
			
		}
		
		#region Properties
		public int ProgressBarValue {
			get{
				return _progressBarValue;
			}
			set{
				_progressBarValue = value;
				OnPropertyChanged("ProgressBarValue");
			}
		}
		
		public ObservableCollection<string> Logs
		{
			get
			{
				return _logs;
			}
		}
		
		#endregion
		
		#region Command
		//построение комманд
		protected override List<CommandViewModel> CreateCommands()
		{
			return new List<CommandViewModel>
			{
				new CommandViewModel("Прервать",
				                     new RelayCommand(param => Abort())),
			};
		}

		#endregion
		
		#region Private
		void Abort()
		{
			App.Current.Shutdown();
			
		}
		#endregion

	}
}
