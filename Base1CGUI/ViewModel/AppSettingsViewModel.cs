/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 03/09/2011
 * Время: 09:08
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Windows.Input;

using _1C;
using SA1CService;

namespace Base1CGUI.ViewModel
{
	/// <summary>
	/// Description of AppSettingsViewModel.
	/// </summary>
	public class AppSettingsViewModel: WorkspaceViewModel
	{
		ServiceSA1C _serviceSA1C;
		ICommand _saveCommand;
		
		public AppSettingsViewModel(WorkspaceViewModel mainWindow, ServiceSA1C serviceSA1C)
		{
			base.MainWindow = mainWindow;
			_serviceSA1C = serviceSA1C;
		}
		
		#region Property
		
		public bool SendErrorMessage{
			get{
				return _serviceSA1C.config.SendErrorMessage;
			}
			set{
				_serviceSA1C.config.SendErrorMessage = value;
			}
		}
		
		public bool CheckUpdate{
			get{
				return _serviceSA1C.config.CheckUpdate;
			}
			set{
				_serviceSA1C.config.CheckUpdate = value;
			}
		}
		
		public bool SendSuccessMessage{
			get{
				return _serviceSA1C.config.SendSuccessMessage;
			}
			set{
				_serviceSA1C.config.SendSuccessMessage = value;
			}
		}
		
		public TypeKillUsers DisableUser{
			get{
				return _serviceSA1C.config.DisableUser;
			}
			set{
				_serviceSA1C.config.DisableUser = value;
			}
		}
		
		/// <summary>
        /// Перечисление версий 1С
        /// </summary>
        public Array TypeKill
        {
            get
            {
                return System.Enum.GetValues(typeof(TypeKillUsers));
            }
        }
		
		public bool DinamycUpdateDB{
			get{
				return _serviceSA1C.config.DinamycUpdateDB;
			}
			set{
				_serviceSA1C.config.DinamycUpdateDB = value;
			}
		}
		
		public EmailSetting Email{
			get{
				return _serviceSA1C.config.Email;
			}
		}
		
		public ServiceHostSetting HostSetting{
			get{
				return _serviceSA1C.config.HostSetting;
			}
		}
		
		#endregion
		
		#region Commands
		/// <summary>
		/// Запись конфигурации в файл
		/// </summary>
		public ICommand SaveCommand
		{
			get
			{
				if (_saveCommand == null)
				{
					_saveCommand = new RelayCommand(param => this.Save());
				}
				return _saveCommand;
			}
		}
		
		protected override List<CommandViewModel> CreateCommands()
		{
			return new List<CommandViewModel>
			{
				new CommandViewModel("Сохранить",
				                     this.SaveCommand),
				
				new CommandViewModel("Отмена",
				                     this.CloseCommand),
			};
		}
		
		/// <summary>
		/// Запись настройки в xml
		/// </summary>
		public void Save()
		{
			_serviceSA1C.SaveSettings();
			//и выйдем
			CloseCommand.Execute(null);
		}
		
		#endregion
		
	}
}
