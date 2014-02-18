/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 03/05/2011
 * Время: 14:44
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;

namespace Base1CGUI.ViewModel
{
	/// <summary>
	/// Description of AboutViewModel.
	/// </summary>
	public class AboutViewModel:WorkspaceViewModel
	{
		public AboutViewModel(WorkspaceViewModel mainWindow)
		{
			base.MainWindow = mainWindow;
			DisplayName = "О программе";
		}
		
		protected override List<CommandViewModel> CreateCommands()
		{
			return new List<CommandViewModel>
			{
				new CommandViewModel("Проверить обновления",
				                     new RelayCommand(param => Update.CheckUpdate())),
				new CommandViewModel("Назад",
				                     this.CloseCommand),
			};
		}
	}
}
