using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Base1CGUI.ViewModel
{
    
    public class SelectTypeViewModel: WorkspaceViewModel
    {
  
        public SelectTypeViewModel(WorkspaceViewModel mainWindow)
        {
            base.MainWindow = mainWindow;
            IsFileDB = true;
            DisplayName = "Выберите тип БД";
        }

        public bool IsFileDB { get; set; }
        public bool IsServerDB { get; set; }

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
            if (IsFileDB)
            {
                FileDBViewModel fileDBVM = new FileDBViewModel(base.MainWindow);
                base.Workspaces = fileDBVM;
            }
            else
            {
                ServerDBViewModel ServerDBVM = new ServerDBViewModel(base.MainWindow);
                base.Workspaces = ServerDBVM;
            }
        }
    }
}
