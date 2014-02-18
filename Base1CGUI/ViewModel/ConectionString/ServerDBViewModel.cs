using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Base1CGUI.ViewModel
{
    class ServerDBViewModel: WorkspaceViewModel
    {
        string _serverName;
        string _baseName;

        public ServerDBViewModel(WorkspaceViewModel mainWindow)
        {
            base.MainWindow = mainWindow;
            //укажем путь из настройки
            if (mainWindow is BaseConfigViewModel)
            {
                _serverName = (mainWindow as BaseConfigViewModel).baseInfo.ServerName;
                _baseName = (mainWindow as BaseConfigViewModel).baseInfo.BaseName;
            }
            DisplayName = "Укажите сервер 1С";
        }

        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                _serverName = value;
                base.OnPropertyChanged("ServerName");
            }
        }

        public string BaseName
        {
            get
            {
                return _baseName;
            }
            set
            {
                _baseName = value;
                base.OnPropertyChanged("BaseName");
            }
        }
       
        protected override List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel("Далее",
                    new RelayCommand(param => Next())),
                
                new CommandViewModel("Отмена",
                    this.CloseCommand),
            };
        }

        void Next()
        {
            //формируем строку соединения
            StringBuilder sb = new StringBuilder("Srvr=\"");
            sb.Append(ServerName);
            sb.Append("\";Ref=");
            sb.Append(BaseName);
            sb.Append("\";");
            (MainWindow as BaseConfigViewModel).ConnectionString = sb.ToString();
            //и закрываем две панели
            CloseAnyPage(2);
        }
    }
}
