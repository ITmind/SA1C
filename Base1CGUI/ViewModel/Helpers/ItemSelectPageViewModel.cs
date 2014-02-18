using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Base1CGUI.ViewModel
{
    class ItemSelectPageViewModel:WorkspaceViewModel
    {
        ObservableCollection<object> _items;
        
        string _propertyName;

        /// <summary>
        /// Создает панель выбора элемента из списка
        /// </summary>
        /// <param name="mainWindow">родительская панель</param>
        /// <param name="Property">Свойство для возврата значения</param>
        /// <param name="items">Список элементов</param>
        /// /// <param name="caption">Заголовок списка</param>
        public ItemSelectPageViewModel(WorkspaceViewModel mainWindow, string propertyName, List<object> items, string caption)
        {
            base.MainWindow = mainWindow;
            _items = new ObservableCollection<object>(items);
            _propertyName = propertyName;
            DisplayName = caption;
        }

        public ObservableCollection<object> Items
        {
            get
            {
                return _items;
            }
        }

        
        public object SelectedItem { get; set; }

        protected override List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel("Выбор",
                    new RelayCommand(param => Select())),
                
                new CommandViewModel("Отмена",
                    this.CloseCommand),
            };
        }

        void Select()
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(MainWindow)[_propertyName];
            if (property != null)
            {
                property.SetValue(MainWindow, SelectedItem);
            }

            CloseCommand.Execute(null);
        }
    }
}
