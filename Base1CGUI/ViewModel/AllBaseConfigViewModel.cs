using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using _1C;
using SA1CService;

namespace Base1CGUI.ViewModel
{
	public class AllBaseConfigViewModel : WorkspaceViewModel
	{
		#region Fields

		ServiceSA1C _serviceSA1C;
		BaseConfigViewModel _currentConfig;
		
		#endregion // Fields

		#region Constructor

		public AllBaseConfigViewModel(WorkspaceViewModel mainWindow, ServiceSA1C serviceSA1C)
		{
			if (serviceSA1C == null)
				throw new ArgumentNullException("serviceSA1C");

			_serviceSA1C = serviceSA1C;
			_serviceSA1C.config.AddConfigEvent += OnAddConfig;
			base.MainWindow = mainWindow;
			MainWindow.DisplayName = "Список настроек обмена";
			// Subscribe for notifications of when a new customer is saved.
			//_customerRepository.CustomerAdded += this.OnCustomerAddedToRepository;

			// Populate the AllCustomers collection with CustomerViewModels.
			this.CreateAllBaseConfig();

			//проверим обновления
			if(_serviceSA1C.config.CheckUpdate){
				ThreadStart start = () => Update.CheckUpdate();

				Thread exchangeTread =  new Thread(start);
				//завершаем поток принудительно при завершении основного потока
				exchangeTread.IsBackground = true;
				exchangeTread.Start();
				
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			_serviceSA1C.LoadSettings();
			this.CreateAllBaseConfig();
			CurrentConfig = null;
			foreach (CommandViewModel command in Commands)
				{
					if (command.DisplayName == "Изменить настройку" || command.DisplayName == "Удалить настройку")
					{
						command.IsEnabled = false;
					}
				}
		}
		
		void CreateAllBaseConfig()
		{
			List<BaseConfigViewModel> all =
				(from cust in _serviceSA1C.config.basesConfig
				 select new BaseConfigViewModel(this, cust, _serviceSA1C)).ToList();

			foreach (BaseConfigViewModel cvm in all)
				cvm.PropertyChanged += this.OnCustomerViewModelPropertyChanged;

			this.AllConfigs = new ObservableCollection<BaseConfigViewModel>(all);
			this.AllConfigs.CollectionChanged += this.OnCollectionChanged;
		}

		#endregion // Constructor

		#region Public Interface

		/// <summary>
		/// Returns a collection of all the CustomerViewModel objects.
		/// </summary>
		public ObservableCollection<BaseConfigViewModel> AllConfigs { get; private set; }

		public BaseConfigViewModel CurrentConfig
		{
			get { return _currentConfig; }
			set
			{
				_currentConfig = value;
			}
		}

		#endregion // Public Interface

		#region  Base Class Overrides

		protected override void OnDispose()
		{
			foreach (BaseConfigViewModel custVM in this.AllConfigs)
				custVM.Dispose();

			this.AllConfigs.Clear();
			this.AllConfigs.CollectionChanged -= this.OnCollectionChanged;

			//_serviceSA1C.CustomerAdded -= this.OnCustomerAddedToRepository;
		}


		#endregion // Base Class Overrides

		#region Event Handling Methods

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null && e.NewItems.Count != 0)
				foreach (BaseConfigViewModel custVM in e.NewItems)
					custVM.PropertyChanged += this.OnCustomerViewModelPropertyChanged;

			if (e.OldItems != null && e.OldItems.Count != 0)
				foreach (BaseConfigViewModel custVM in e.OldItems)
					custVM.PropertyChanged -= this.OnCustomerViewModelPropertyChanged;
		}

		void OnCustomerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "IsSelected")
			{
				return;
			}

			if ((sender as BaseConfigViewModel).IsSelected)
			{

				CurrentConfig = (BaseConfigViewModel)sender;
				foreach (CommandViewModel command in Commands)
				{
					if (command.DisplayName == "Изменить настройку" || command.DisplayName == "Удалить настройку")
					{
						command.IsEnabled = true;
					}
				}
			}
			//(sender as BaseConfigViewModel).VerifyPropertyName(IsSelected);
		}

		void OnAddConfig(object sender, AddConfigEventArgs e)
		{
			AllConfigs.Add(new BaseConfigViewModel(this,e.baseConfig,_serviceSA1C));
		}

		#endregion // Event Handling Methods

		#region Commands

		protected override List<CommandViewModel> CreateCommands()
		{
			return new List<CommandViewModel>
			{
				new CommandViewModel(
					"Добавить настройку",
					new RelayCommand(param => this.AddNewSetting())),
				
				new CommandViewModel(
					"Изменить настройку",
					new RelayCommand(param => this.EditSetting()), false),

				new CommandViewModel(
					"Удалить настройку",
					new RelayCommand(param => this.RemoveSetting())),
				
				new CommandViewModel(
					"Настроки SA1C",
					new RelayCommand(param => this.ShowAppSettings())),
				
				new CommandViewModel(
					"О программе",
					new RelayCommand(param => this.ShowAbout())),
				
				new CommandViewModel(
					"Закрыть",
					MainWindow.CloseCommand)
			};
		}

		
		#endregion // Commands

		#region Private Helpers

		void AddNewSetting()
		{
			AddNewDBViewModel workspace = new AddNewDBViewModel(this, Helper1C.GetBasesFromAddData(), _serviceSA1C);
			//BaseConfig newBaseConfig = new BaseConfig();
			//BaseConfigViewModel workspace = new BaseConfigViewModel(this, newBaseConfig, _serviceSA1C);
			base.Workspaces = workspace;
		}

		void EditSetting()
		{
			base.Workspaces = CurrentConfig;
		}

		void RemoveSetting()
		{
			var result = MessageBox.Show("Вы уверенны что хотите удалить настройку " + CurrentConfig.DisplayName + " ?",
			                             "Удаление настройки",
			                             MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes)
			{
				AllConfigs.Remove(CurrentConfig);
				_serviceSA1C.config.basesConfig.Remove(CurrentConfig.baseConfig);
				_serviceSA1C.SaveSettings();
			}
		}

		void ShowAbout()
		{
			AboutViewModel workspace = new AboutViewModel(this);
			base.Workspaces = workspace;
		}
		
		void ShowAppSettings()
		{
			AppSettingsViewModel workspace = new AppSettingsViewModel(this,_serviceSA1C);
			base.Workspaces = workspace;
		}
		
		#endregion // Private Helpers
	}
}