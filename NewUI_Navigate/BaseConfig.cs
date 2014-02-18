using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SA1CService;
using _1C;

namespace NewUI_Navigate
{
    class BaseConfigElemen
    {
         #region Fields

        BaseConfig _baseConfig;
        ServiceSA1C _serviceSA1C;
        bool _isSelected;
   
        #endregion // Fields

        #region Constructor

        public BaseConfigElemen(ServiceSA1C serviceSA1C)
        {
            if (baseConfig == null)
                throw new ArgumentNullException("baseConfig");
            _serviceSA1C = serviceSA1C;
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

    }
}
