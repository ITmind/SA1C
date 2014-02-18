using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Windows.Input;

namespace Base1CGUI.ViewModel
{
    class FileDBViewModel: WorkspaceViewModel
    {
        ICommand _selectPachDB;
        string _pathToDB;

        public FileDBViewModel(WorkspaceViewModel mainWindow)
        {
            base.MainWindow = mainWindow;
            //укажем путь из настройки
            if (mainWindow is BaseConfigViewModel)
            {
                _pathToDB = (mainWindow as BaseConfigViewModel).baseInfo.PathToDB;
            }
            DisplayName = "Укажите путь к БД";

        }

        public string PachToDB
        {
            get
            {
                return _pathToDB;
            }
            set
            {
                _pathToDB = value;
                base.OnPropertyChanged("PachToDB");
            }
        }

        public ICommand SelectPachDB
        {
            get
            {
                if (_selectPachDB == null)
                {
                    _selectPachDB = new RelayCommand(param => this.SelectFolder());
                }
                return _selectPachDB;
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
            StringBuilder sb =new StringBuilder("File=\"");
            sb.Append(PachToDB);
            sb.Append("\";");
            (MainWindow as BaseConfigViewModel).ConnectionString = sb.ToString();
            //и закрываем две панели
            CloseAnyPage(2);
        }

        void SelectFolder()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                PachToDB = Path.GetDirectoryName(ofd.FileName);
            }
        }

    }
}
