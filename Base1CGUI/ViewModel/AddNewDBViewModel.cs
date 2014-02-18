using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using _1C;
using Base1CGUI.View;
using SA1CService;

namespace Base1CGUI.ViewModel
{
    class AddNewDBViewModel: WorkspaceViewModel
    {
        ObservableCollection<BaseInfo> _listDB;

        ServiceSA1C _serviceSA1C;

        //TODO: Доабвить вывод строки соединения под списком баз, как в 1с
        
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="items"></param>
        /// <param name="serviceSA1C"></param>
        /// <summary>
        /// Создает панель выбора элемента из списка
        /// </summary>
        /// <param name="mainWindow">родительская панель</param>
        /// <param name="items">Список элементов</param>
        public AddNewDBViewModel(WorkspaceViewModel mainWindow, List<BaseInfo> items, ServiceSA1C serviceSA1C)
        {
            base.MainWindow = mainWindow;
            _listDB = new ObservableCollection<BaseInfo>(items);            
            IsExestisDB = true;
            _serviceSA1C = serviceSA1C;
            DisplayName = "Добавление новой настройки";
        }

        public ObservableCollection<BaseInfo> ListDB
        {
            get
            {
                return _listDB;
            }
        }


        public bool IsExestisDB { get; set; }
        public bool IsManual { get; set; }

        public BaseInfo SelectedItem { get; set; }

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
            BaseConfigViewModel workspace = null;

            if (IsManual)
            {
                BaseConfig newBaseConfig = new BaseConfig();
                workspace = new BaseConfigViewModel(this, newBaseConfig, _serviceSA1C);                
            }
            else
            {
                if (SelectedItem != null)
                {
                    BaseConfig baseConfig = new BaseConfig();
                    baseConfig.baseInfo = SelectedItem;
                    baseConfig.Name = SelectedItem.Name;
                    workspace = new BaseConfigViewModel(this, baseConfig, _serviceSA1C);
                }
            }

            if (workspace != null)
            {
                CloseCommand.Execute(true);
                base.Workspaces = workspace;

            }
        }
    }
}
