using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using _1C;
using Microsoft.Win32;
using SA1CService;

namespace Base1CGUI.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a Customer object.
    /// </summary>
    public class BaseConfigViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        #region Fields

        BaseConfig _baseConfig;
        ServiceSA1C _serviceSA1C;
        bool _isSelected;
        RelayCommand _saveCommand;
        RelayCommand _selectConectionString;
        RelayCommand _selectPlan;
        RelayCommand _selectNode;
        RelayCommand _startExchange;
        RelayCommand _selectRulesPatch;
   
        #endregion // Fields

        #region Constructor

        public BaseConfigViewModel(WorkspaceViewModel mainWindow, BaseConfig baseConfig, ServiceSA1C serviceSA1C)
        {
            if (baseConfig == null)
                throw new ArgumentNullException("baseConfig");

            _baseConfig = baseConfig;
            _serviceSA1C = serviceSA1C;
            base.MainWindow = mainWindow;
        }

        #endregion // Constructor

        #region BaseConfig Properties

        /// <summary>
        /// Конфигурация
        /// </summary>
        public BaseConfig baseConfig
        {
            get
            {
                return _baseConfig;
            }
        }

        /// <summary>
        /// Имя настройки
        /// </summary>
        public string SettingName
        {
            get
            {
               return _baseConfig.Name;
            }

            set
            {
                _baseConfig.Name = value;
                _baseConfig.baseInfo.Name = value;
                DisplayName = value;        
            }
        }

        /// <summary>
        /// Адрес сервера
        /// </summary>
        public string IP
        {
            get { return _baseConfig.IP; }
            set
            {
                if (value == _baseConfig.IP)
                    return;

                _baseConfig.IP = value;

                base.OnPropertyChanged("IP");
            }
        }
        
        /// <summary>
        /// Порт сервера
        /// </summary>
        public string Port
        {
            get { return _baseConfig.Port; }
            set
            {
                if (value == _baseConfig.Port)
                    return;

                _baseConfig.Port = value;

                base.OnPropertyChanged("Port");
            }
        }

        /// <summary>
        /// Порт сервера
        /// </summary>
        public string FilenameRules
        {
            get { return _baseConfig.filenameRules; }
            set
            {
                if (value == _baseConfig.filenameRules)
                    return;

                _baseConfig.filenameRules = value;

                base.OnPropertyChanged("FilenameRules");
            }
        }
        
        public bool IsCentralDB{
        	get{ return _baseConfig.IsCentralDB;}
        	set{
        		_baseConfig.IsCentralDB = value;
        	}
        }
        /// <summary>
        /// Название плана обмена
        /// </summary>
        public string NameOfPlan
        {
            get { return _baseConfig.NameOfPlan; }
            set
            {
                if (value == _baseConfig.NameOfPlan)
                    return;

                _baseConfig.NameOfPlan = value;

                base.OnPropertyChanged("NameOfPlan");
            }
        }

        /// <summary>
        /// Код/название узла обмена
        /// </summary>
        public string CodeOfNode
        {
            get { return _baseConfig.CodeOfNode; }
            set
            {
                if (value == _baseConfig.CodeOfNode)
                    return;

                _baseConfig.CodeOfNode = value;

                base.OnPropertyChanged("CodeOfNode");
            }
        }

        /// <summary>
        /// СОМ строка соединения
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return baseInfo.ConnectionString;
            }

            set
            {
                baseInfo.ConnectionString = value;
                base.OnPropertyChanged("ConnectionString");
            }

        }

        /// <summary>
        /// Настройки БД
        /// </summary>
        public BaseInfo baseInfo
        {
            get
            {
                return _baseConfig.baseInfo;
            }
        }

        /// <summary>
        /// Последняя ошибка
        /// </summary>
        public string LastError
        {
            get
            {
            	string res = "";
            	if(_baseConfig.status.jobStatus == JobStatus.Error){
            		res = _baseConfig.status.description;	
            	}
            	
                return res;
            }
        }

        /// <summary>
        /// Последний успешный обмен
        /// </summary>
        public string LastExchangeDate
        {
            get
            {
            	string[] s = _baseConfig.LastExchangeDate.GetDateTimeFormats('F');
            	return s[0];
            }
        }

        
        public string Expression{
        	get{
        		return _baseConfig.JobSheduler.Expression;
        	}
        	set{
        		_baseConfig.JobSheduler.Expression = value;
        	}
        }
        
        public bool IsShedulerEnable{
        	get{
        		return _baseConfig.JobSheduler.isEnable;
        	}
        	set{
        		_baseConfig.JobSheduler.isEnable = value;
        	}
        }
        #endregion // Customer Properties

        #region Presentation Properties

           
        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Перечисление версий 1С
        /// </summary>
        public Array EnumVersion
        {
            get
            {
                return System.Enum.GetValues(typeof(EnumVersion1C));
            }
        }

        #endregion // Presentation Properties

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

        public ICommand StartExchangeCommand
        {
            get
            {
                if (_startExchange == null)
                {
                    _startExchange = new RelayCommand(param => this.StartExchange());
                }
                return _startExchange;
            }
        }

        /// <summary>
        /// Открытие панели выбора пути к БД
        /// </summary>
        public ICommand SelectConectionString
        {
            get
            {
                if (_selectConectionString == null)
                {
                    _selectConectionString = new RelayCommand(param => this.SelectConnString());
                }
                return _selectConectionString;
            }
        }
        
        /// <summary>
        /// Открытие панели выбора пути к БД
        /// </summary>
        public ICommand SelectRulesPatch
        {
            get
            {
                if (_selectRulesPatch == null)
                {
                    _selectRulesPatch = new RelayCommand(param => this.SelectRulesFile());
                }
                return _selectRulesPatch;
            }
        }

        /// <summary>
        /// Открытие панели выбора плана обмена
        /// </summary>
        public ICommand SelectPlan
        {
            get
            {
                if (_selectPlan == null)
                {
                    _selectPlan = new RelayCommand(param => this.SelectPlanName());
                }
                return _selectPlan;
            }
        }

        /// <summary>
        /// Открытие панели выбора узла обмена
        /// </summary>
        public ICommand SelectNode
        {
            get
            {
                if (_selectNode == null)
                {
                    _selectNode = new RelayCommand(param => this.SelectNodeName());
                }
                return _selectNode;
            }
        }

        #endregion
                

        #region Public Methods

        /// <summary>
        /// Запись настройки в xml
        /// </summary>
        public void Save()
        {
            if (this.IsNewSetting)
                _serviceSA1C.config.AddConfig(_baseConfig);
            _serviceSA1C.SaveSettings();
            //и выйдем
            CloseCommand.Execute(null);
        }

        #endregion // Public Methods

        #region Private Helpers

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

        void SelectConnString()
        {
            SelectTypeViewModel st = new SelectTypeViewModel(this);
            base.Workspaces = st;
        }
        
        void SelectRulesFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                FilenameRules = ofd.FileName;
            }
        }

        void SelectPlanName()
        {
        	//проверим, все ли заполнено
        	if(baseInfo.User == String.Empty){
        		MessageBox.Show("Не указанно имя пользователя");
        		return;
        	}
        	
            List<object> planList = new List<object>();
           
            //получаем планы обмена
            using (_1Cv8 v8 = new _1Cv8(baseInfo))
            {
                try
                {
                    v8.Connect();
                    foreach (dynamic plan in v8.Object1C.Метаданные.ПланыОбмена)
                    {
                        planList.Add(plan.Имя);
                    }

                    ItemSelectPageViewModel itemSelectPage = new ItemSelectPageViewModel(this, "NameOfPlan", planList, "Укажите план обмена:");
                    base.Workspaces = itemSelectPage;

                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                    //обрабатываем ошибку
                }
            }
           
        }

        void SelectNodeName()
        {
            if (NameOfPlan == String.Empty)
            {
                MessageBox.Show("Не указан план обмена");
                return;
            }

            List<object> nodeList = new List<object>();

            //получаем планы обмена
            using (_1Cv8 v8 = new _1Cv8(baseInfo))
            {
                try
                {
                    
                    v8.Connect();
                    //dynamic result = v8.Object1C.ПланыОбмена.Get("Полный");//.Выбрать();
                    dynamic refPlane = v8.GetProperty(v8.Object1C.ПланыОбмена, NameOfPlan);
                    dynamic thisNode = refPlane.ЭтотУзел();
                    dynamic result = refPlane.Выбрать();
                    while (result.Следующий())   //ПланыОбмена
                    {
                        if (thisNode.Наименование != result.Наименование)
                        {
                            nodeList.Add(result.Наименование);
                            //nodeList.Add(new { result.Наименование, result.Код });
                        }
                    }


                    //TODO: придумать как передавать в список наименования, а получать код
                    ItemSelectPageViewModel itemSelectPage = new ItemSelectPageViewModel(this, "CodeOfNode", nodeList, "Укажите узел обмена:");
                    base.Workspaces = itemSelectPage;

                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                    //обрабатываем ошибку
                }
            }

        }

        
        bool IsNewSetting
        {
            get { return ! _serviceSA1C.config.basesConfig.Contains(_baseConfig); }
        }

        #endregion // Private Helpers

        void StartExchange()
        {
            //string name = BaseConfig.Name;
            ExchangeProcessViewModel exchangeProcessVM=new ExchangeProcessViewModel(this.MainWindow,_serviceSA1C, baseConfig);
           	this.Workspaces = exchangeProcessVM; 
        }

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return "Error"; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        #endregion // IDataErrorInfo Members
    }
}