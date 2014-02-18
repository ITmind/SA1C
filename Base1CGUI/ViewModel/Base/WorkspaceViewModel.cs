using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Base1CGUI.ViewModel
{
    /// <summary>
    /// Аргументы для события закрытия
    /// Позволяет указать количество закрываемых панелей
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        public CloseEventArgs(int numPageForClose)
        {
            this.numPageForClose = numPageForClose;
        }

        public int numPageForClose;
    }

    /// <summary>
    /// Базовый класс для всех панелей
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        #region Fields

        RelayCommand _closeCommand;
        ObservableCollection<CommandViewModel> _commands;
        //protected WorkspaceViewModel _mainWindow;

        #endregion // Fields

        #region Constructor

        protected WorkspaceViewModel()
        {
        }

        #endregion // Constructor

        #region CloseCommand

        /// <summary>
        /// Команда закрытия одной текущей панели
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => this.OnRequestClose());

                return _closeCommand;
            }
        }

        #endregion // CloseCommand

        #region RequestClose [event]

        public delegate void CloseEventHandler(object sender, CloseEventArgs e);
        
        /// <summary>
        /// Сообщает, что панель должна быть закрыта
        /// </summary>
        public event CloseEventHandler RequestClose;

        void OnRequestClose()
        {
            CloseEventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, new CloseEventArgs(1));
        }

        /// <summary>
        /// Закрытие нескольких панелей
        /// </summary>
        /// <param name="numPageForClose">количество закрываемых панелей</param>
        public void CloseAnyPage(int numPageForClose)
        {
            CloseEventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, new CloseEventArgs(numPageForClose));
        }

        #endregion // RequestClose [event]

        /// <summary>
        /// Ссылка на родительскую, для данной панели, панель.
        /// </summary>
        public virtual WorkspaceViewModel MainWindow { get; set; }

        public virtual void Refresh(){
        	//переопределяем на панелях нуждаюхися в обновлении отображения
        }
        /// <summary>
        /// Контейнер, содержащий все панели.
        /// Свойство должно быть обязательно переопределено в самой главной панели
        /// </summary>
        public virtual object Workspaces
        {
            get
            {
                return MainWindow.Workspaces;
            }
            set
            {
                MainWindow.Workspaces = value;
            }
        }

        public override string DisplayName
        {
            get
            {
                return MainWindow.DisplayName;
            }
            set
            {
                MainWindow.DisplayName = value;
            }
        }
        
        /// <summary>
        /// Команды, доступные для данной панели
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ObservableCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
            set
            {
                _commands = value;
                base.OnPropertyChanged("Commands");
            }
        }

        /// <summary>
        /// Создает команды для панели
        /// Должна быть преопределена в каждой конкретной панели
        /// </summary>
        /// <returns></returns>
        protected virtual List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>();
        }
    }
}